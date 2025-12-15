using Microsoft.EntityFrameworkCore;
using Models;
using PostService.Data;
using PostService.DTOs;
using PostService.Specifications;
using PostService.Utils;
using JiebaNet.Segmenter;
using LinqKit;
using Oracle.ManagedDataAccess.Client;

namespace PostService.Repositories
{
    public interface IPostRepository
    {
        Task<List<Post>> GetPostsByIdsAsync(List<long> ids);
        Task<Post?> GetByIdAsync(long postId);
        Task<List<Post>> GetPostsByUserIdAsync(long userId);
        Task MarkAsDeletedAsync(long postId);
        // refactoring begin
        Task<Post> InsertPostAsync(Post post, List<long> tags);
        // refactoring end
        Task<List<string>> GetTagNamesByPostIdAsync(long postId);
        Task<int?> IncrementViewsAsync(long postId, CancellationToken ct = default);
        Task<List<Post>> GetByAuthorNotDeletedAsync(long authorId, CancellationToken ct = default);
        Task<(bool Exists, bool Owned, bool Updated, int CurrentHidden)> SetHiddenAsync(long postId, long ownerId, bool next, CancellationToken ct = default);
        Task<List<Post>> GetPagedAsync(long? lastId, int num, bool desc = true, int PostMode = 0, string? tagName = null);
        Task<(bool Exists, bool Owned, bool Allowed, bool Updated, string? ReasonConflict, Post? Post)> UpdatePostAsync(PostUpdateRequest req, long editorUserId, CancellationToken ct = default);
        /// <summary>
        /// 使用 Oracle Text CONTAINS 做全文候选检索，返回按 oracle_score 降序的候选（不做最终排序）
        /// maxCandidates: 若为 null 则返回全部 Oracle Text 命中的结果（慎用）
        /// </summary>
        Task<List<Post>> SearchCandidatesByOracleTextAsync(string query, int? maxCandidates);

        Task<List<SearchSuggestResponse>> SearchPostsByTitleAsync(string keyword, int limits);
        Task<List<SearchSuggestResponse>> SearchPostsByContentAsync(string keyword, int limits);
        Task<List<SearchSuggestResponse>> SearchTagsAsync(string keyword, int limits);
        Task<List<SearchSuggestResponse>> SearchUsersAsync(string keyword, int limits);
        Task<List<SearchSuggestResponse>> SearchCirclesAsync(string keyword, int limits);
    }

    public class PostRepository : IPostRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public PostRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }


        public async Task<List<Post>> GetPostsByIdsAsync(List<long> ids)
        {
            await using var context = _contextFactory.CreateDbContext();
            var posts = await context.Posts
                .Where(p => ids.Contains(p.PostId) && p.IsDeleted == 0)
                .ToListAsync();

            if (!posts.Any()) return posts;

            // 一次性查询所有 PostTag + TagName
            var postTags = await (from pt in context.PostTags
                                  join t in context.Tags on pt.TagId equals t.TagId
                                  where ids.Contains(pt.PostId)
                                  select new
                                  {
                                      pt.PostId,
                                      t.TagName
                                  }).ToListAsync();

            // 用 Dictionary 分组映射
            var tagLookup = postTags
                .GroupBy(x => x.PostId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.TagName).ToList());

            // 附加到 Post 对象
            foreach (var post in posts)
            {
                post.TagNames = tagLookup.TryGetValue(post.PostId, out var tags)
                    ? tags
                    : new List<string>();
            }

            return posts;
        }

        public async Task<Post?> GetByIdAsync(long postId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.Posts.FindAsync(postId);
        }

        public async Task<List<Post>> GetPostsByUserIdAsync(long userId)
        {
            await using var context = _contextFactory.CreateDbContext();
            var posts = await context.Posts
                .Where(p => p.UserId == userId)
                .Include(p => p.PostTags)       // 包含联接表实体 (PostTag)
                .ThenInclude(pt => pt.Tag)      // 基于联接表实体，继续包含实际的 Tag 实体
                .ToListAsync();

            if (!posts.Any()) return posts;

            // 数据已全部加载到内存中，现在为每个帖子填充 [NotMapped] 的 TagNames 属性
            foreach (var post in posts)
            {
                if (post.PostTags != null)
                {
                    post.TagNames = post.PostTags
                        .Select(pt => pt.Tag?.TagName) // 从已加载的 Tag 实体中选择标签名
                        .Where(tagName => !string.IsNullOrEmpty(tagName)) // 过滤掉可能存在的 null 或空字符串
                        .ToList()!; // 将最终的名称列表赋值给 TagNames 属性
                }
            }

            return posts;
        }

        public async Task MarkAsDeletedAsync(long postId)
        {
            await using var context = _contextFactory.CreateDbContext();
            var post = await context.Posts.FindAsync(postId);
            if (post != null)
            {
                post.IsDeleted = 1;
                context.Posts.Update(post);
                await context.SaveChangesAsync();
            }
        }

        // refactoring begin
        public async Task<Post> InsertPostAsync(Post post, List<long> tags)
        {
            await using var context = _contextFactory.CreateDbContext();

            // 获取 tagNames
            var tagNames = new List<string>();
            if (tags != null && tags.Count > 0)
            {
                var tagEntities = await context.Tags
                    .Where(t => tags.Contains(t.TagId))
                    .ToListAsync();
                tagNames = tagEntities.Select(t => t.TagName).ToList();
            }

            // 获取用户名称
            var user = await context.Users
                .Where(u => u.UserId == post.UserId)
                .Select(u => u.Username)
                .FirstOrDefaultAsync();

            // 获取圈子名称
            var circle = await context.Circles
                .Where(c => c.CircleId == post.CircleId)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();

            // 生成 SearchText
            post.SearchText =
                $"<TITLE>{post.Title}</TITLE>" +
                $"<CONTENT>{post.Content}</CONTENT>" +
                $"<TAGS>{string.Join(" ", tagNames)}</TAGS>" +
                $"<USER>{user}</USER>" +
                $"<CIRCLE>{circle}</CIRCLE>";

            context.Posts.Add(post);
            await context.SaveChangesAsync(); // 生成 PostId

            // 插入 PostTag
            if (tags != null && tags.Count > 0)
            {
                foreach (var tagId in tags)
                {
                    context.PostTags.Add(new PostTag
                    {
                        PostId = post.PostId,
                        TagId = tagId
                    });
                }
                await context.SaveChangesAsync();
            }

            await context.Entry(post).ReloadAsync();
            return post;
        }
        // refactoring end

        public async Task<List<string>> GetTagNamesByPostIdAsync(long postId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.PostTags
                .Where(pt => pt.PostId == postId)
                .Include(pt => pt.Tag)
                .Select(pt => pt.Tag.TagName)
                .ToListAsync();
        }

        public async Task<List<Post>> GetPagedAsync(
            long? lastId,
            int num,
            bool desc = true,
            int PostMode = 0,
            string? tagName = null)   // 新增参数: 按标签筛选
        {
            await using var context = _contextFactory.CreateDbContext();

            // 使用规格模式构建基础查询: 可见帖子 + 可选标签过滤
            var baseSpec = new PostSpecificationCollection(
                new VisiblePostSpecification(),
                new TagFilterSpecification(tagName)
            );

            // 应用规格后得到基础查询(已经包含未删除、未隐藏等规则)
            IQueryable<Post> q = baseSpec.Apply(context.Posts);

            // 排序逻辑
            switch (PostMode)
            {
                case 1: // 按浏览量 Views 排序
                    if (lastId.HasValue && lastId.Value > 0)
                    {
                        var lastPost = await context.Posts
                            .Where(p => p.PostId == lastId.Value)
                            .Select(p => new { p.Views, p.PostId })
                            .FirstOrDefaultAsync();

                        if (lastPost != null)
                        {
                            q = q.Where(p =>
                                (p.Views ?? 0) < (lastPost.Views ?? 0) ||
                                ((p.Views ?? 0) == (lastPost.Views ?? 0) && p.PostId < lastPost.PostId));
                        }
                    }

                    q = q.OrderByDescending(p => p.Views ?? 0)
                        .ThenByDescending(p => p.PostId);
                    break;

                case 2: // 按点赞 Likes 排序
                    if (lastId.HasValue && lastId.Value > 0)
                    {
                        var lastPost = await context.Posts
                            .Where(p => p.PostId == lastId.Value)
                            .Select(p => new { p.Likes, p.PostId })
                            .FirstOrDefaultAsync();

                        if (lastPost != null)
                        {
                            q = q.Where(p =>
                                (p.Likes ?? 0) < (lastPost.Likes ?? 0) ||
                                ((p.Likes ?? 0) == (lastPost.Likes ?? 0) && p.PostId < lastPost.PostId));
                        }
                    }

                    q = q.OrderByDescending(p => p.Likes ?? 0)
                        .ThenByDescending(p => p.PostId);
                    break;

                case 3: // 首页推荐(热度综合公式): 热度分数 = 浏览量 * 0.5 + 点赞 * 0.3 – 踩 * 0.2
                    if (lastId.HasValue && lastId.Value > 0)
                    {
                        var lastPost = await context.Posts
                            .Where(p => p.PostId == lastId.Value)
                            .Select(p => new
                            {
                                p.PostId,
                                Score = (p.Views ?? 0) * 0.5
                                      + (p.Likes ?? 0) * 0.3
                                      - (p.Dislikes ?? 0) * 0.2
                            })
                            .FirstOrDefaultAsync();

                        if (lastPost != null)
                        {
                            q = q.Where(p =>
                                ((p.Views ?? 0) * 0.5
                                 + (p.Likes ?? 0) * 0.3
                                 - (p.Dislikes ?? 0) * 0.2) < lastPost.Score
                                || (((p.Views ?? 0) * 0.5
                                     + (p.Likes ?? 0) * 0.3
                                     - (p.Dislikes ?? 0) * 0.2) == lastPost.Score
                                    && p.PostId < lastPost.PostId));
                        }
                    }

                    q = q.OrderByDescending(p =>
                            (p.Views ?? 0) * 0.5
                          + (p.Likes ?? 0) * 0.3
                          - (p.Dislikes ?? 0) * 0.2)
                        .ThenByDescending(p => p.PostId);
                    break;

                case 4: // 发现页推荐(最新帖子)
                    if (lastId.HasValue && lastId.Value > 0)
                    {
                        q = q.Where(p => p.PostId < lastId.Value);
                    }

                    q = q.OrderByDescending(p => p.CreatedAt ?? DateTime.MinValue)
                        .ThenByDescending(p => p.PostId);
                    break;

                default: // 按时间(PostId)
                    if (desc)
                    {
                        if (lastId.HasValue && lastId.Value > 0)
                            q = q.Where(p => p.PostId < lastId.Value);

                        q = q.OrderByDescending(p => p.PostId);
                    }
                    else
                    {
                        if (lastId.HasValue && lastId.Value > 0)
                            q = q.Where(p => p.PostId > lastId.Value);

                        q = q.OrderBy(p => p.PostId);
                    }
                    break;
            }

            var posts = await q.Take(num).ToListAsync();

            if (!posts.Any())
                return posts;

            var postIds = posts.Select(p => p.PostId).ToList();

            // 查询所有相关的标签
            var postTags = await (from pt in context.PostTags
                                  join t in context.Tags on pt.TagId equals t.TagId
                                  where postIds.Contains(pt.PostId)
                                  select new { pt.PostId, t.TagName }).ToListAsync();

            var tagLookup = postTags
                .GroupBy(x => x.PostId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.TagName).ToList());

            foreach (var p in posts)
            {
                if (tagLookup.TryGetValue(p.PostId, out var tags))
                {
                    p.TagNames = tags;
                }
            }

            return posts;
        }

        public async Task<int?> IncrementViewsAsync(long postId, CancellationToken ct = default)
        {
            await using var ctx = _contextFactory.CreateDbContext();

            // ① 原子自增（Oracle）
            var affected = await ctx.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE POST SET VIEWS=NVL(VIEWS,0)+1 WHERE POST_ID={postId}", ct);
            if (affected == 0) return null; // 不存在该帖子

            // ② 读回最新值（便于前端拿到当前阅读数；也可不返回）
            var current = await ctx.Posts
                .Where(p => p.PostId == postId)
                .Select(p => (int?)((p.Views ?? 0)))
                .FirstOrDefaultAsync(ct);

            return current;
        }

        public async Task<List<Post>> GetByAuthorNotDeletedAsync(long authorId, CancellationToken ct = default)
        {
            await using var ctx = _contextFactory.CreateDbContext();

            // 仅取本人且未删除的帖子；排序按 PostId 倒序（= 最新优先）
            var query = ctx.Posts
                .Where(p => (p.UserId ?? -1) == authorId && p.IsDeleted == 0)
                .OrderByDescending(p => p.PostId);

            var posts = await query.ToListAsync(ct);
            if (posts.Count == 0) return posts;

            // 一次性取回标签并回填（与现有风格一致）
            var ids = posts.Select(p => p.PostId).ToList();
            var postTags = await (
                from pt in ctx.PostTags
                join t in ctx.Tags on pt.TagId equals t.TagId
                where ids.Contains(pt.PostId)
                select new { pt.PostId, t.TagName }
            ).ToListAsync(ct);

            var map = postTags
                .GroupBy(x => x.PostId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.TagName).ToList());

            foreach (var p in posts)
                if (map.TryGetValue(p.PostId, out var tags))
                    p.TagNames = tags;

            return posts;
        }

        public async Task<List<Post>> SearchCandidatesByOracleTextAsync(string query, int? maxCandidates)
        {
            await using var context = _contextFactory.CreateDbContext();
            // 空 query 的分支：返回未删除/未隐藏的帖子（按 CreatedAt 降序），并限制条数（如果提供了 maxCandidates）
            if (string.IsNullOrWhiteSpace(query))
            {
                // 对于空查询，仍然返回最新帖子
                return await context.Posts
                    .Where(p => p.IsDeleted == 0 && p.IsHidden == 0)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(maxCandidates ?? 50) // 默认值
                    .ToListAsync();
            }

            // 1. 使用 Jieba.NET 对查询进行分词
            var tokens = JiebaUtil.Tokenize(query).ToList();

            // 如果分词后没有有效词语，返回空集合
            if (!tokens.Any())
            {
                return new List<Post>();
            }

            // 2. 构建 SQL 条件
            var whereConditions = new List<string>();
            var sqlParameters = new List<object>();

            for (int i = 0; i < tokens.Count; i++)
            {
                string paramName = $":p{i}";
                whereConditions.Add($"Title LIKE '%' || {paramName} || '%' OR Content LIKE '%' || {paramName} || '%'");
                sqlParameters.Add(new OracleParameter(paramName, tokens[i]));
            }

            string sql = $@"
SELECT *
FROM Post
WHERE Is_Deleted = 0 AND Is_Hidden = 0
  AND ({string.Join(" OR ", whereConditions)})
ORDER BY Post_Id
";

            var posts = await context.Posts
                .FromSqlRaw(sql, sqlParameters.ToArray())
                .ToListAsync();

            return posts;
        }

        /// <summary>
        /// 根据关键词从帖子标题中搜索建议
        /// </summary>
        public async Task<List<SearchSuggestResponse>> SearchPostsByTitleAsync(string keyword, int limits)
        {
            await using var context = _contextFactory.CreateDbContext();

            // 使用可见帖子规格,确保搜索结果中不包含已删除/已隐藏的帖子
            var visibleSpec = new VisiblePostSpecification();
            var baseQuery = visibleSpec.Apply(context.Posts);

            return await baseQuery
                .Where(p => p.Title != null && p.Title.Contains(keyword))
                .OrderByDescending(p => p.Views) // 假设按浏览量排序,更热门的排在前面
                .Take(limits)
                .Select(p => new SearchSuggestResponse
                {
                    Keyword = p.Title!,
                    Type = SearchSuggestResponse.KeywordType.Title
                })
                .ToListAsync();
        }

        /// <summary>
        /// 根据关键词从帖子正文中搜索建议
        /// </summary>
        public async Task<List<SearchSuggestResponse>> SearchPostsByContentAsync(string keyword, int limits)
        {
            await using var context = _contextFactory.CreateDbContext();

            // 使用可见帖子规格,确保搜索结果中不包含已删除/已隐藏的帖子
            var visibleSpec = new VisiblePostSpecification();
            var baseQuery = visibleSpec.Apply(context.Posts);

            return await baseQuery
                .Where(p => p.Content != null && p.Content.Contains(keyword))
                .OrderByDescending(p => p.Views) // 假设按浏览量排序
                .Take(limits)
                .Select(p => new SearchSuggestResponse
                {
                    Keyword = p.Content!.Length > 50
                        ? p.Content.Substring(0, 50) + "..."
                        : p.Content,
                    Type = SearchSuggestResponse.KeywordType.Content
                })
                .ToListAsync();
        }

        /// <summary>
        /// 根据关键词从标签中搜索建议
        /// </summary>
        public async Task<List<SearchSuggestResponse>> SearchTagsAsync(string keyword, int limits)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.Tags
                .Where(t => t.TagName != null && t.TagName.Contains(keyword))
                .OrderByDescending(t => t.Count) // 假设按使用次数排序
                .Take(limits)
                .Select(t => new SearchSuggestResponse
                {
                    Keyword = t.TagName,
                    Type = SearchSuggestResponse.KeywordType.Tag
                })
                .ToListAsync();
        }

        /// <summary>
        /// 根据关键词从用户中搜索建议
        /// </summary>
        public async Task<List<SearchSuggestResponse>> SearchUsersAsync(string keyword, int limits)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.Users
                .Where(u => u.Username != null && u.Username.Contains(keyword))
                .OrderByDescending(u => u.Posts.Count) // 假设按发帖数排序
                .Take(limits)
                .Select(u => new SearchSuggestResponse
                {
                    Keyword = u.Username,
                    Type = SearchSuggestResponse.KeywordType.User
                })
                .ToListAsync();
        }

        /// <summary>
        /// 根据关键词从圈子中搜索建议
        /// </summary>
        public async Task<List<SearchSuggestResponse>> SearchCirclesAsync(string keyword, int limits)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.Circles
                .Where(c => c.Name != null && c.Name.Contains(keyword))
                .OrderByDescending(c => c.Posts.Count) // 假设按圈子内的帖子数排序
                .Take(limits)
                .Select(c => new SearchSuggestResponse
                {
                    Keyword = c.Name,
                    Type = SearchSuggestResponse.KeywordType.Circle
                })
                .ToListAsync();
        }

        public async Task<(bool Exists, bool Owned, bool Updated, int CurrentHidden)> SetHiddenAsync(
            long postId, long ownerId, bool next, CancellationToken ct = default)
        {
            await using var context = _contextFactory.CreateDbContext();

            // 先查最小必要信息：是否存在、是否本人、是否已删除、当前隐藏值
            var row = await context.Posts
                .Where(p => p.PostId == postId)
                .Select(p => new { p.UserId, IsDeleted = p.IsDeleted, IsHidden = p.IsHidden })
                .FirstOrDefaultAsync(ct);

            if (row == null)
                return (Exists: false, Owned: false, Updated: false, CurrentHidden: 0); // 404 不存在

            var isOwner = (row.UserId ?? -1) == ownerId;
            if (!isOwner)
                return (Exists: true, Owned: false, Updated: false, CurrentHidden: row.IsHidden); // 403 非本人

            if (row.IsDeleted != 0)
                return (Exists: true, Owned: true, Updated: false, CurrentHidden: row.IsHidden); // 已删除，不允许改

            var target = next ? 1 : 0;
            if (row.IsHidden == target)
                return (Exists: true, Owned: true, Updated: false, CurrentHidden: row.IsHidden); // 幂等：无需写库

            // 原子更新（本人 + 未删除）
            var pVal = new OracleParameter("p_val", OracleDbType.Int32) { Value = target };
            var pId = new OracleParameter("p_id", OracleDbType.Int64) { Value = postId };
            var pUid = new OracleParameter("p_uid", OracleDbType.Int64) { Value = ownerId };

            var sql = "UPDATE POST SET IS_HIDDEN = :p_val " +
                      "WHERE POST_ID = :p_id AND USER_ID = :p_uid AND NVL(IS_DELETED,0) = 0";

            var affected = await context.Database.ExecuteSqlRawAsync(sql, new[] { pVal, pId, pUid }, ct);

            return (Exists: true, Owned: true, Updated: affected > 0, CurrentHidden: target);
        }

        public async Task<(bool Exists, bool Owned, bool Allowed, bool Updated, string? ReasonConflict, Post? Post)>
            UpdatePostAsync(PostUpdateRequest req, long editorUserId, CancellationToken ct = default)
        {
            await using var ctx = _contextFactory.CreateDbContext();

            // 1) 基本信息 & 权限检查
            var row = ctx.Posts
                .Where(p => p.PostId == req.PostId)
                .Select(p => new
                {
                    p.PostId,
                    p.UserId,
                    IsDeleted = p.IsDeleted,
                    IsHidden = p.IsHidden
                })
                .FirstOrDefaultAsync(ct);

            if (row == null) return (Exists: false, Owned: false, Allowed: false, Updated: false, ReasonConflict: null, Post: null);

            var owned = (row.Result.UserId ?? -1) == editorUserId;
            if (!owned) return (Exists: true, Owned: false, Allowed: false, Updated: false, ReasonConflict: null, Post: null);

            // 允许编辑：未删除；隐藏与否不影响帖主编辑
            var allowed = (row.Result.IsDeleted == 0);
            if (!allowed) return (Exists: true, Owned: true, Allowed: false, Updated: false, ReasonConflict: "Post deleted", Post: null);

            // 2) 开启事务：更新主表 + 标签（差异）
            using var tx = await ctx.Database.BeginTransactionAsync(ct);

            // 2.1 更新 POST（Title/Content/CircleId/UpdatedAt/可选 SearchText）
            var pId = new OracleParameter("p_id", OracleDbType.Int64) { Value = req.PostId };
            var pTitle = new OracleParameter("p_title", OracleDbType.NVarchar2) { Value = (object)(req.Title ?? "") };
            var pContent = new OracleParameter("p_content", OracleDbType.NClob) { Value = (object)(req.Content ?? "") };
            var pCircle = new OracleParameter("p_circle", OracleDbType.Int64) { Value = (object?)req.CircleId ?? DBNull.Value };

            var sqlUpdate = @"UPDATE POST
                              SET TITLE = :p_title,
                                  CONTENT = :p_content,
                                  CIRCLE_ID = :p_circle
                              WHERE POST_ID = :p_id";
            var affected = await ctx.Database.ExecuteSqlRawAsync(sqlUpdate, new[] { pTitle, pContent, pCircle, pId }, ct);
            if (affected == 0) { await tx.RollbackAsync(ct); return (true, true, false, false, "Update failed", null); }

            // 2.2 标签差异更新（以 tagId 列表为准）
            // 现有 tagId 集合
            var currentTagIds = await (from pt in ctx.PostTags
                                       where pt.PostId == req.PostId
                                       select pt.TagId).ToListAsync(ct);

            var newTagIds = (req.Tags ?? new()).Distinct().ToList();

            var toRemove = currentTagIds.Except(newTagIds).ToList();
            var toAdd = newTagIds.Except(currentTagIds).ToList();

            if (toRemove.Count > 0)
            {
                await ctx.Database.ExecuteSqlRawAsync(
                    "DELETE FROM POSTTAG WHERE POST_ID = :p_id AND TAG_ID IN (" +
                    string.Join(",", toRemove.Select((_, i) => $":t{i}")) + ")",
                    new OracleParameter[] { pId }.Concat(
                        toRemove.Select((id, i) => new OracleParameter($"t{i}", OracleDbType.Int64) { Value = id })
                    ).ToArray(), ct);
                // 可选：同步 TAG.COUNT -= removedCount（如你们有维护）
            }

            if (toAdd.Count > 0)
            {
                // 批量插入（简单起见逐条；如需批量可用数组绑定）
                foreach (var tagId in toAdd)
                {
                    var pTag = new OracleParameter("p_tag", OracleDbType.Int64) { Value = tagId };
                    await ctx.Database.ExecuteSqlRawAsync(
                        "INSERT INTO POSTTAG(POST_ID, TAG_ID) VALUES(:p_id, :p_tag)",
                        new OracleParameter[] { pId, pTag }, ct);
                    // 可选：同步 TAG.COUNT += 1
                }
            }

            await tx.CommitAsync(ct);

            // 3) 读回最新帖子（含 TagNames）
            var post = await ctx.Posts.FirstOrDefaultAsync(p => p.PostId == req.PostId, ct);
            if (post != null)
            {
                var tags = await (from pt in ctx.PostTags
                                  join t in ctx.Tags on pt.TagId equals t.TagId
                                  where pt.PostId == req.PostId
                                  select t.TagName).ToListAsync(ct);
                post.TagNames = tags;
            }

            return (Exists: true, Owned: true, Allowed: true, Updated: true, ReasonConflict: null, Post: post);
        }
    }
}