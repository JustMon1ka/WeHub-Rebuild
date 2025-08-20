using System.Security.Claims;
using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostService.DTOs;
using PostService.Services;

namespace PostService.Controllers;

[ApiController]
[Route("api/posts")]
public class PostController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly ICommentService _commentService;

    public PostController(IPostService postService, ICommentService commentService)
    {
        _postService = postService;
        _commentService = commentService;
    }
    
    [HttpGet]
    public async Task<BaseHttpResponse<List<PostResponse>>> GetPosts([FromQuery] string ids)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                return BaseHttpResponse<List<PostResponse>>.Fail(400, "参数 ids 不能为空");
            }

            List<long> idList;
            try
            {
                idList = ids
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => long.Parse(id.Trim()))
                    .ToList();
            }
            catch (FormatException)
            {
                return BaseHttpResponse<List<PostResponse>>.Fail(400, "参数格式错误，应为逗号分隔的数字列表");
            }

            var posts = await _postService.GetPostsByIdsAsync(idList);

            if (posts == null || posts.Count == 0)
            {
                return BaseHttpResponse<List<PostResponse>>.Fail(404, "未找到指定的帖子");
            }

            var responseList = posts.Select(post => new PostResponse
            {
                PostId = post.PostId,
                UserId = post.UserId ?? 0,
                Title = post.Title ?? "",
                Content = post.Content ?? "",
                Tags = post.TagNames,  // 使用附加的 Tags
                CreatedAt = post.CreatedAt ?? DateTime.MinValue,
                Views = post.Views ?? 0,
                Likes = post.Likes ?? 0
            }).ToList();

            return BaseHttpResponse<List<PostResponse>>.Success(responseList);
        }
        catch (Exception ex)
        {
            // 记录日志（推荐注入 ILogger<PostController> ）
            return BaseHttpResponse<List<PostResponse>>.Fail(500, $"服务器内部错误：{ex.Message}");
        }
    }
    
    [HttpDelete]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<BaseHttpResponse<object?>> DeletePost([FromQuery] long post_id)
    {
        try
        {
            // 从JWT token中提取用户ID（你可能需要配置Claims）
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return BaseHttpResponse<object?>.Fail(401, "未认证的用户");
            }

            var userId = long.Parse(userIdClaim.Value);

            // 检查帖子是否存在且属于当前用户
            var post = await _postService.GetPostByIdAsync(post_id);
            if (post == null || post.IsDeleted == 1)
            {
                return BaseHttpResponse<object?>.Fail(404, "帖子不存在或已删除");
            }

            if (post.UserId != userId)
            {
                return BaseHttpResponse<object?>.Fail(403, "无权限删除他人帖子");
            }

            // 执行逻辑删除
            await _postService.DeletePostAsync(post_id);

            return BaseHttpResponse<object?>.Success(null, "删除成功");
        }
        catch (Exception ex)
        {
            // 可记录日志
            return BaseHttpResponse<object?>.Fail(500, "服务器内部错误：" + ex.Message);
        }
    }
    
    [HttpGet("{post_id}")]
    public async Task<BaseHttpResponse<PostResponse>> GetPost([FromRoute] long post_id){
        try
        {
            var post = await _postService.GetPostByIdAsync(post_id);

            if (post == null || post.IsDeleted == 1)
            {
                return BaseHttpResponse<PostResponse>.Fail(404, "帖子不存在");
            }

            // ✅ 调用 Service 获取标签名
            var tagNames = await _postService.GetTagsByPostIdAsync(post_id);

            var response = new PostResponse
            {
                PostId = post.PostId,
                UserId = post.UserId ?? 0,
                Title = post.Title ?? "",
                Content = post.Content ?? "",
                Tags = tagNames,   // ✅ 填充标签
                CreatedAt = post.CreatedAt ?? DateTime.MinValue,
                Views = post.Views ?? 0,
                Likes = post.Likes ?? 0,
                CircleId = post.CircleId
            };

            return BaseHttpResponse<PostResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return BaseHttpResponse<PostResponse>.Fail(500, "服务器内部错误：" + ex.Message);
        }
    }

    [HttpPost("publish")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<BaseHttpResponse<PostPublishResponse>> PublishPost(
        [FromBody] PostPublishRequest postPublishRequest)
    {
        try
        {
            // 从 token 中提取 userId
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return BaseHttpResponse<PostPublishResponse>.Fail(401, "无法从Token中提取用户信息");
            }

            long userId = long.Parse(userIdClaim.Value);
            
            // 如果未填 CircleId，用 100000 作为默认值
            long circleId = postPublishRequest.CircleId ?? 100000;

            // 发布帖子
            var post = await _postService.PublishPostAsync(userId,
                circleId,
                postPublishRequest.Title ?? string.Empty,
                postPublishRequest.Content ?? string.Empty,
                postPublishRequest.Tags ?? new List<long>());

            return BaseHttpResponse<PostPublishResponse>.Success(new PostPublishResponse
            {
                PostId = post.PostId,
                CreatedAt = post.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""
            });
        }
        catch (Exception ex)
        {
            return BaseHttpResponse<PostPublishResponse>.Fail(500, "发帖失败: " + ex.Message);
        }
    }

    [HttpGet("search")]
    public async Task<BaseHttpResponse<List<SearchResponse>>> SearchPosts([FromQuery] int? limits,
        [FromQuery] string? query)
    {
        try
        {
            var results = await _postService.SearchPostsAsync(query, limits);
            return BaseHttpResponse<List<SearchResponse>>.Success(results);
        }
        catch (Exception ex)
        {
            // 记日志
            return BaseHttpResponse<List<SearchResponse>>.Fail(500, "Search failed: " + ex.Message);
        }
    }

    [HttpGet("search/suggest")]
    public async Task<BaseHttpResponse<List<SearchSuggestResponse>>> SearchSuggest([FromQuery] int? limits,
        [FromQuery] string? keyword)
    {
        // 设置默认值
        int effectiveLimits = limits ?? 10;
        
        // 简单参数验证
        if (effectiveLimits <= 0)
        {
            return BaseHttpResponse<List<SearchSuggestResponse>>.Fail(400, "limits must be a positive number.");
        }

        try
        {
            var suggestions = await _postService.GetSearchSuggestionsAsync(keyword, effectiveLimits);
            return BaseHttpResponse<List<SearchSuggestResponse>>.Success(suggestions);
        }
        catch (Exception ex)
        {
            // 记录异常日志
            // ...
            return BaseHttpResponse<List<SearchSuggestResponse>>.Fail(500, "An error occurred while retrieving search suggestions.");
        }
    }

    [HttpPost("comment")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<BaseHttpResponse<CommentResponse>> Comment([FromBody] CommentRequest commentRequest)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return BaseHttpResponse<CommentResponse>.Fail(401, "未认证的用户");
        }

        var userId = long.Parse(userIdClaim.Value);
        return await _commentService.AddCommentOrReplyAsync(userId, commentRequest);
    }

    [HttpDelete("comment")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<BaseHttpResponse<object?>> DeleteComment([FromQuery] long id, [FromQuery] CommentRequest.CommentType type)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return BaseHttpResponse<object?>.Fail(401, "未认证的用户");
        }

        var userId = long.Parse(userIdClaim.Value);
        return await _commentService.DeleteCommentOrReplyAsync(userId, id, type);
    }

    [HttpGet("comment")]
    public async Task<BaseHttpResponse<List<GetCommentResponse>>> GetComments([FromQuery] long postId)
    {
        return await _commentService.GetCommentsAsync(postId);
    }
}