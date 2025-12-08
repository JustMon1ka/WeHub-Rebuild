using System.Text.RegularExpressions;
using DTOs;
using PostService.DTOs;
using Models;
using PostService.Repositories;
using JiebaNet.Segmenter;
using PostService.Command;
using PostService.Factory;
using PostService.Utils;
using PostService.Validator;

namespace PostService.Services
{
    public interface IPostService
    {
        Task<List<Post>> GetPostsByIdsAsync(List<long> ids);
        Task<Post?> GetPostByIdAsync(long postId);
        Task DeletePostAsync(long postId);
        Task<BaseHttpResponse<PostPublishResponse>> PublishAsync(PublishPostCommand command);
        Task<List<string>> GetTagsByPostIdAsync(long postId);
        Task<List<SearchResponse>> SearchPostsAsync(string? query, int? limits);
        Task<List<SearchSuggestResponse>> GetSearchSuggestionsAsync(string? keyword, int limits);
        Task<List<Post>> GetPostsByUserIdAsync(long userId);
        Task<int?> IncrementViewsAsync(long postId, CancellationToken ct=default);
        Task<(bool Exists, bool Owned, bool Updated, int CurrentHidden)> SetHiddenAsync(long postId, long ownerId, bool next, CancellationToken ct = default);
        Task<List<Post>> GetMyPostsAsync(long authorId, CancellationToken ct = default);
        Task<List<Post>> GetPagedPostsAsync(long? lastId, int num, bool desc = true, int PostMode = 0, string? tagName = null);
        Task<(int Code, string? Error, PostResponse? Data)> UpdatePostAsync(PostUpdateRequest req, long editorUserId, CancellationToken ct = default);
    }
    
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IPostRedisRepository _postRedisRepository;

        public PostService(IPostRepository postRepository, IPostRedisRepository postRedisRepository)
        {
            _postRepository = postRepository;
            _postRedisRepository = postRedisRepository;
        }

        public async Task<List<Post>> GetPostsByIdsAsync(List<long> ids)
        {
            return await _postRepository.GetPostsByIdsAsync(ids);
        }
        
        public async Task<Post?> GetPostByIdAsync(long postId)
        {
            return await _postRepository.GetByIdAsync(postId);
        }

        public async Task DeletePostAsync(long postId)
        {
            await _postRepository.MarkAsDeletedAsync(postId);
        }
        
        /// <summary>
        /// 发布帖子（Command + Template Method）
        /// 将校验、默认值处理、仓储调用全部封装
        /// </summary>
        public async Task<BaseHttpResponse<PostPublishResponse>> PublishAsync(PublishPostCommand command)
        {
            var request = command.Request;

            // 1. 构建责任链
            var validatorChain = new TitleValidator();
            validatorChain.SetNext(new ContentValidator());

            // 2. 校验
            string? validation = validatorChain.Validate(request);
            if (validation != null)
                return BaseHttpResponse<PostPublishResponse>.Fail(400, validation);

            // 3. 默认值处理
            long circleId = request.CircleId ?? 100000;

            // 4. 使用工厂创建实体
            var post = PostFactory.Create(
                command.UserId,
                circleId,
                request.Title!,
                request.Content!
            );

            List<long> tags = request.Tags ?? new();

            // 5. 调用 Repository
            var inserted = await _postRepository.InsertPostAsync(post, tags);

            // 6. DTO 返回
            var response = new PostPublishResponse
            {
                PostId = inserted.PostId,
                CreatedAt = inserted.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""
            };

            return BaseHttpResponse<PostPublishResponse>.Success(response);
        }
        
        /// <summary>
        /// 发布帖子请求的校验（可扩展）
        /// </summary>
        private string? ValidatePublishRequest(PostPublishRequest request)
        {
            if (request == null)
                return "请求体为空";

            if (string.IsNullOrWhiteSpace(request.Title))
                return "标题不能为空";

            if (string.IsNullOrWhiteSpace(request.Content))
                return "内容不能为空";

            return null; // 校验通过
        }
        
        public async Task<List<string>> GetTagsByPostIdAsync(long postId)
        {
            return await _postRepository.GetTagNamesByPostIdAsync(postId);
        }
        
        // BM25 参数
        private const double k1 = 1.5;
        private const double b = 0.75;

        public async Task<List<SearchResponse>> SearchPostsAsync(string? query, int? limits)
        {
            // 1) 从数据库中检索 Top M 候选，基于 LIKE 匹配
            int candidateMax = 800; // 经验值，可调整
            var docs = await _postRepository.SearchCandidatesByOracleTextAsync(query ?? string.Empty, candidateMax);

            // 若没有 query 或没有找到任何文档，直接返回
            if (string.IsNullOrWhiteSpace(query) || docs == null || !docs.Any())
            {
                var all = docs
                    .Select(x => MapToSearchResponse(x))
                    .OrderByDescending(x => x.PostId)
                    .ToList();
                return limits.HasValue && limits.Value > 0 ? all.Take(limits.Value).ToList() : all;
            }

            // 2) 分词（为 BM25 计算）
            var qTokens = JiebaUtil.Tokenize(query).Where(t => t.Length > 0).Distinct().ToList();
            if (!qTokens.Any()) qTokens.Add(query.ToLowerInvariant());

            // 3) 为候选集合构建文档词频（tf）与文档长度（dl），同时统计 df（在候选集合上）
            int N = docs.Count;
            var docTermFreqs = new List<Dictionary<string, int>>();
            var docLengths = new List<int>();

            for (int i = 0; i < docs.Count; i++)
            {
                var p = docs[i];
                string text = BuildTextForScoring(p);
                var tokens = JiebaUtil.Tokenize(text).ToList();
                var tf = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var t in tokens)
                {
                    if (t.Length == 0) continue;
                    tf.TryGetValue(t, out int c);
                    tf[t] = c + 1;
                }

                docTermFreqs.Add(tf);
                docLengths.Add(tokens.Count);
            }

            // df per term (number of docs containing term)
            var df = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in qTokens)
            {
                int cnt = docTermFreqs.Count(d => d.ContainsKey(t));
                df[t] = cnt;
            }

            double avgdl = docLengths.Count > 0 ? docLengths.Average() : 0.0;

            // 4) 计算 BM25 分数
            var scored = new List<(Post post, double score)>();
            for (int i = 0; i < docs.Count; i++)
            {
                double score = 0.0;
                var tf = docTermFreqs[i];
                int dl = docLengths[i];

                // k1 和 b 应该在类的顶部作为常量或字段定义
                double k1 = 1.2;
                double b = 0.75;

                foreach (var term in qTokens)
                {
                    if (!df.TryGetValue(term, out int dfterm) || dfterm == 0) continue;
                    int fqd = tf.TryGetValue(term, out var f) ? f : 0;
                    double idf = Math.Log((N - dfterm + 0.5) / (dfterm + 0.5) + 1.0);
                    double denom = fqd + k1 * (1.0 - b + b * dl / Math.Max(1.0, avgdl));
                    double numer = fqd * (k1 + 1.0);
                    score += idf * (denom == 0 ? 0 : numer / denom);
                }

                scored.Add((docs[i], score));
            }

            // 5) 最终排序：BM25 降序，Tie break 用 CreatedAt
            var ordered = scored
                .OrderByDescending(x => x.score)
                .ThenByDescending(x => x.post.CreatedAt ?? DateTime.MinValue)
                .Select(x => MapToSearchResponse(x.post))
                .ToList();

            if (limits.HasValue && limits.Value > 0) ordered = ordered.Take(limits.Value).ToList();

            // 只有当搜索结果不为空时才更新 Redis，避免无效搜索污染数据
            if (ordered.Any())
            {
                await _postRedisRepository.IncrementSearchCountAsync(query);
            }

            return ordered;
        }

        private static string BuildTextForScoring(Post p)
        {
            // BM25F 风格：给 title 和 tags 更高权重，可通过重复字段或拼接带标记来模拟
            var title = p.Title ?? "";
            var content = p.Content ?? "";
            var username = p.User?.Username ?? "";
            var circle = p.Circle?.Name ?? "";
            var tags = p.PostTags?.Select(pt => pt.Tag?.TagName ?? "").Where(t => !string.IsNullOrEmpty(t)).ToList() ?? new List<string>();

            // 重复 title 与 tags 来增加权重（简单方法），更专业的是基于 BM25F 的字段权重
            string weighted = string.Join(" ", Enumerable.Repeat(title, 3)); // title 权重扩大 3 倍
            weighted += " " + string.Join(" ", Enumerable.Repeat(string.Join(" ", tags), 3)); // tags 权重
            weighted += " " + content;
            weighted += " " + username + " " + circle;
            return weighted;
        }

        private static SearchResponse MapToSearchResponse(Post p)
        {
            return new SearchResponse
            {
                PostId = p.PostId,
                Title = p.Title ?? string.Empty,
                Content = p.Content ?? string.Empty,
                Tags = p.PostTags?.Select(pt => pt.Tag?.TagName ?? string.Empty).Where(s=>!string.IsNullOrEmpty(s)).Distinct().ToList() ?? new List<string>()
            };
        }
        
        public async Task<List<SearchSuggestResponse>> GetSearchSuggestionsAsync(string? keyword, int limits)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                // keyword为空，从 Redis 获取热门搜索
                return await GetHotSearchesAsync(limits);
            }

            // keyword不为空，综合搜索
            var suggestions = new List<SearchSuggestResponse>();

            // 从多个源异步获取数据
            var titleSuggestionsTask = _postRepository.SearchPostsByTitleAsync(keyword, limits);
            var contentSuggestionsTask = _postRepository.SearchPostsByContentAsync(keyword, limits);
            var tagSuggestionsTask = _postRepository.SearchTagsAsync(keyword, limits);
            var userSuggestionsTask = _postRepository.SearchUsersAsync(keyword, limits);
            var circleSuggestionsTask = _postRepository.SearchCirclesAsync(keyword, limits);
            var hotSuggestionsTask = GetHotSearchesAsync(limits);

            // 等待所有任务完成
            await Task.WhenAll(
                titleSuggestionsTask,
                contentSuggestionsTask,
                tagSuggestionsTask,
                userSuggestionsTask,
                circleSuggestionsTask,
                hotSuggestionsTask);

            // 整合所有结果
            suggestions.AddRange(titleSuggestionsTask.Result);
            suggestions.AddRange(contentSuggestionsTask.Result);
            suggestions.AddRange(tagSuggestionsTask.Result);
            suggestions.AddRange(userSuggestionsTask.Result);
            suggestions.AddRange(circleSuggestionsTask.Result);
            suggestions.AddRange(hotSuggestionsTask.Result);

            // 去重并截取
            var distinctSuggestions = suggestions
                .GroupBy(s => new { s.Keyword, s.Type })
                .Select(g => g.First())
                .Take(limits)
                .ToList();
                
            // 记录搜索事件，用于更新 Redis 热门搜索
            await _postRedisRepository.IncrementSearchCountAsync(keyword);
            
            return distinctSuggestions;
        }

        private async Task<List<SearchSuggestResponse>> GetHotSearchesAsync(int limits)
        {
            var hotKeywords = await _postRedisRepository.GetHotKeywordsAsync(limits);
            return hotKeywords
                .Select(kw => new SearchSuggestResponse
                {
                    Keyword = kw,
                    Type = SearchSuggestResponse.KeywordType.Other
                })
                .ToList();
        }
        
        public async Task<List<Post>> GetPostsByUserIdAsync(long userId)
        {
            return await _postRepository.GetPostsByUserIdAsync(userId);
        }
        
        public async Task<List<Post>> GetPagedPostsAsync(long? lastId, int num, bool desc = true, int PostMode = 0, string? tagName = null)
        {
            // 兜底：限制 num 防止一次取过多
            if (num <= 0)
            {
                num = 10;
            }
            else if (num > 20)
            {
                num = 20;
            }
            return await _postRepository.GetPagedAsync(lastId, num, desc, PostMode, tagName);
        }
        
        public Task<(bool Exists, bool Owned, bool Updated, int CurrentHidden)> SetHiddenAsync(
            long postId, long ownerId, bool next, CancellationToken ct = default)
            => _postRepository.SetHiddenAsync(postId, ownerId, next, ct);
        
        public Task<int?> IncrementViewsAsync(long postId, CancellationToken ct=default)
            => _postRepository.IncrementViewsAsync(postId, ct);
        
        public Task<List<Post>> GetMyPostsAsync(long authorId, CancellationToken ct = default)
            => _postRepository.GetByAuthorNotDeletedAsync(authorId, ct);
        
        // Services/PostService/Services/PostService.cs
        public async Task<(int Code, string? Error, PostResponse? Data)>
            UpdatePostAsync(PostUpdateRequest req, long editorUserId, CancellationToken ct = default)
        {
            // 交给仓储做存在性/权限/删除态检查与实际更新（含标签 diff）
            var r = await _postRepository.UpdatePostAsync(req, editorUserId, ct);

            if (!r.Exists) return (404, null, null);
            if (!r.Owned)  return (403, null, null);
            if (!r.Allowed) return (403, null, null);
            if (!r.Updated && r.ReasonConflict != null) return (409, r.ReasonConflict, null);

            // 映射为对外 DTO
            var p = r.Post!;
            var dto = new PostResponse{
                PostId = p.PostId,
                UserId = p.UserId ?? 0,
                Title = p.Title ?? "",
                Content = p.Content ?? "",
                Tags = p.TagNames ?? new List<string>(),
                CreatedAt = p.CreatedAt ?? DateTime.MinValue,
                Views = p.Views ?? 0,
                Likes = p.Likes ?? 0,
                CircleId = p.CircleId
            };
            return (200, null, dto);
        }
    }
}
