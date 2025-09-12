using Microsoft.EntityFrameworkCore;
using Models;
using PostService.Data;
using PostService.DTOs;
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
        Task<Post> InsertPostAsync(long userId, long circleId, string title, string content, List<long> tags);
        Task<List<string>> GetTagNamesByPostIdAsync(long postId);
        Task<List<Post>> GetPagedAsync(long? lastId, int num, bool desc = true);
        Task<int?> IncrementViewsAsync(long postId, CancellationToken ct=default);
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
            return await context.Posts
                .Where(p => p.UserId == userId)
                .ToListAsync();
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
        
        public async Task<Post> InsertPostAsync(long userId, long circleId, string title, string content, List<long> tags)
        {
            await using var context = _contextFactory.CreateDbContext();
            var post = new Post
            {
                UserId = userId,
                CircleId = (int)circleId,
                Title = title,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = 0,
                IsHidden = 0,
                Views = 0,
                Likes = 0,
                Dislikes = 0,
            };

            // 生成 search_text
            var tagNames = new List<string>();
            if (tags != null && tags.Count > 0)
            {
                var tagEntities = await context.Tags
                    .Where(t => tags.Contains(t.TagId))
                    .ToListAsync();
                tagNames = tagEntities.Select(t => t.TagName).ToList();
            }

            // 可以通过 User / Circle 的导航属性或者直接查询
            var user = await context.Users
                .Where(u => u.UserId == userId)
                .Select(u => u.Username)
                .FirstOrDefaultAsync();

            var circle = await context.Circles
                .Where(c => c.CircleId == circleId)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();

            post.SearchText =
                $"<TITLE>{title}</TITLE>" +
                $"<CONTENT>{content}</CONTENT>" +
                $"<TAGS>{string.Join(" ", tagNames)}</TAGS>" +
                $"<USER>{user}</USER>" +
                $"<CIRCLE>{circle}</CIRCLE>";

            context.Posts.Add(post);
            await context.SaveChangesAsync();  // 一次性插入 Post + search_text，生成 PostId

            // 处理 Tags → POSTTAG
            if (tags != null && tags.Count > 0)
            {
                foreach (var tagId in tags)
                {
                    var postTag = new PostTag
                    {
                        PostId = post.PostId,
                        TagId = tagId
                    };
                    context.Set<PostTag>().Add(postTag);
                }
                await context.SaveChangesAsync();
            }

            await context.Entry(post).ReloadAsync();
            return post;
        }
        
        public async Task<List<string>> GetTagNamesByPostIdAsync(long postId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.PostTags
                .Where(pt => pt.PostId == postId)
                .Include(pt => pt.Tag)
                .Select(pt => pt.Tag.TagName)
                .ToListAsync();
        }
        
        public async Task<List<Post>> GetPagedAsync(long? lastId, int num, bool desc = true)
        {
            await using var context = _contextFactory.CreateDbContext();

            var q = context.Posts.Where(p => p.IsDeleted == 0 && p.IsHidden == 0);

            if (desc)
            {
                if (lastId.HasValue && lastId.Value > 0)
                {
                    q = q.Where(p => p.PostId < lastId.Value);
                }
                q = q.OrderByDescending(p => p.PostId);
            }
            else
            {
                if (lastId.HasValue && lastId.Value > 0)
                {
                    q = q.Where(p => p.PostId > lastId.Value);
                }
                q = q.OrderBy(p => p.PostId);
            }

            // 取一页
            var posts = await q.Take(num).ToListAsync();
            if (!posts.Any())
            {
                return posts;
            }

            // 填充 TagNames（与现有 GetPostsByIdsAsync 的做法保持一致）
            var ids = posts.Select(p => p.PostId).ToList();
            var postTags = await (from pt in context.PostTags
                join t in context.Tags on pt.TagId equals t.TagId
                where ids.Contains(pt.PostId)
                select new { pt.PostId, t.TagName }).ToListAsync();

            var tagLookup = postTags
                .GroupBy(x => x.PostId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.TagName).ToList());

            foreach (var p in posts)
                if (tagLookup.TryGetValue(p.PostId, out var tags)) p.TagNames = tags;

            return posts;
        }
        
        public async Task<int?> IncrementViewsAsync(long postId, CancellationToken ct=default)
        {
            await using var ctx=_contextFactory.CreateDbContext();

            // ① 原子自增（Oracle）
            var affected=await ctx.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE POST SET VIEWS=NVL(VIEWS,0)+1 WHERE POST_ID={postId}", ct);
            if(affected==0) return null; // 不存在该帖子

            // ② 读回最新值（便于前端拿到当前阅读数；也可不返回）
            var current=await ctx.Posts
                .Where(p=>p.PostId==postId)
                .Select(p=>(int?)((p.Views??0)))
                .FirstOrDefaultAsync(ct);

            return current;
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
            return await context.Posts
                .Where(p => p.Title != null && p.Title.Contains(keyword) && p.IsDeleted == 0 && p.IsHidden == 0)
                .OrderByDescending(p => p.Views) // 假设按浏览量排序，更热门的排在前面
                .Take(limits)
                .Select(p => new SearchSuggestResponse
                {
                    Keyword = p.Title,
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
            return await context.Posts
                .Where(p => p.Content != null && p.Content.Contains(keyword) && p.IsDeleted == 0 && p.IsHidden == 0)
                .OrderByDescending(p => p.Views) // 假设按浏览量排序
                .Take(limits)
                .Select(p => new SearchSuggestResponse
                {
                    // 截取部分内容作为建议，避免过长
                    Keyword = p.Content!.Length > 50 ? p.Content.Substring(0, 50) + "..." : p.Content,
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
    }
}