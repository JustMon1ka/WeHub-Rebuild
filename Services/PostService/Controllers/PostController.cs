using System.Security.Claims;
using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using PostService.DTOs;
using PostService.Services;
using Microsoft.Extensions.Caching.Distributed;
using PostService.Command;

namespace PostService.Controllers;

[ApiController]
[Route("api/posts")]
public class PostController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly ICommentService _commentService;
    private readonly IShareService _shareService;
    private readonly ILikeService _likeService;

    public PostController(IPostService postService, ICommentService commentService, IShareService shareService, ILikeService likeService)
    {
        _postService = postService;
        _commentService = commentService;
        _shareService = shareService;
        _likeService = likeService;
    }

    [HttpGet]
    public async Task<BaseHttpResponse<List<PostResponse>>> GetPosts([FromQuery] string? ids, [FromQuery] long? userId)
    {
        try
        {
            // 检查参数冲突：不能同时提供 ids 和 userId
            if (!string.IsNullOrWhiteSpace(ids) && userId.HasValue)
            {
                return BaseHttpResponse<List<PostResponse>>.Fail(400, "不能同时指定 ids 和 userId 参数");
            }

            // 如果提供了 ids，则执行原有逻辑
            if (!string.IsNullOrWhiteSpace(ids))
            {
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

                var postsByIds = await _postService.GetPostsByIdsAsync(idList);

                if (postsByIds == null || postsByIds.Count == 0)
                {
                    return BaseHttpResponse<List<PostResponse>>.Fail(404, "未找到指定的帖子");
                }

                var responseListByIds = postsByIds.Select(ToPostResponse).ToList();
                return BaseHttpResponse<List<PostResponse>>.Success(responseListByIds);
            }


            // 如果提供了 userId，则执行新逻辑
            if (userId.HasValue)
            {
                var postsByUserId = await _postService.GetPostsByUserIdAsync(userId.Value);

                if (postsByUserId == null || postsByUserId.Count == 0)
                {
                    return BaseHttpResponse<List<PostResponse>>.Fail(404, $"未找到用户 ID 为 {userId.Value} 的帖子");
                }

                var responseListByUserId = postsByUserId.Select(ToPostResponse).ToList();
                return BaseHttpResponse<List<PostResponse>>.Success(responseListByUserId);
            }

            // 如果 ids 和 userId 都没有提供
            return BaseHttpResponse<List<PostResponse>>.Fail(400, "参数 ids 和 userId 至少需要提供一个");
        }
        catch (Exception ex)
        {
            // 记录日志（推荐注入 ILogger<PostController> ）
            return BaseHttpResponse<List<PostResponse>>.Fail(500, $"服务器内部错误：{ex.Message}");
        }
    }

    // 辅助方法，用于将 Post 对象映射为 PostResponse 对象，避免代码重复。
    private PostResponse ToPostResponse(Post post)
    {
        return new PostResponse
        {
            PostId = post.PostId,
            UserId = post.UserId ?? 0,
            Title = post.Title ?? "",
            Content = post.Content ?? "",
            Tags = post.TagNames,
            CreatedAt = post.CreatedAt ?? DateTime.MinValue,
            Views = post.Views ?? 0,
            Likes = post.Likes ?? 0
        };
    }

    [HttpGet("list")]
    public async Task<BaseHttpResponse<List<PostResponse>>> List(
        [FromQuery] long? lastId,
        [FromQuery] int? num,
        [FromQuery] int? PostMode,
        [FromQuery] string? tagName)
    {
        try
        {
            int take = num.GetValueOrDefault(10);
            if (take <= 0) take = 10;
            if (take > 100) take = 100;
            
            int take_PostMode = PostMode.GetValueOrDefault(0);
            if (take_PostMode < 0 || take_PostMode > 4) take_PostMode = 0;

            Console.WriteLine($"🔍 Controller 收到 PostMode={PostMode}");

            var posts = await _postService.GetPagedPostsAsync(lastId, take, true, take_PostMode, tagName);

            var data = posts.Select(p => new PostResponse
            {
                PostId = p.PostId,
                UserId = p.UserId ?? 0,
                Title = p.Title ?? "",
                Tags = p.TagNames ?? new List<string>(),
                CreatedAt = p.CreatedAt ?? DateTime.MinValue,
                Views = p.Views ?? 0,
                Likes = p.Likes ?? 0,
                CircleId = p.CircleId
            }).ToList();

            return BaseHttpResponse<List<PostResponse>>.Success(data);
        }
        catch (Exception ex)
        {
            return BaseHttpResponse<List<PostResponse>>.Fail(500, "服务器内部错误：" + ex.Message);
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
    public async Task<BaseHttpResponse<PostResponse>> GetPost([FromRoute] long post_id)
    {
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
                IsHidden = post.IsHidden,
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
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null)
                return BaseHttpResponse<PostPublishResponse>.Fail(401, "无法从 Token 中获取用户信息");

            long userId = long.Parse(userIdStr);

            var command = new PublishPostCommand(postPublishRequest, userId);

            return await _postService.PublishAsync(command);
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

    [HttpGet("comments")]
    public async Task<BaseHttpResponse<List<GetCommentResponse>>> GetComments(
        [FromQuery] long? postId,
        [FromQuery] long? userId)
    {
        try
        {
            // 检查参数冲突：不能同时提供 postId 和 userId
            if (postId.HasValue && userId.HasValue)
            {
                return BaseHttpResponse<List<GetCommentResponse>>.Fail(400, "不能同时指定 postId 和 userId 参数");
            }

            // 如果 postId 有值，执行原有逻辑
            if (postId.HasValue)
            {
                var commentsByPostId = await _commentService.GetCommentsByPostIdAsync(postId.Value);
                return commentsByPostId;
            }

            // 如果 userId 有值，执行新逻辑
            if (userId.HasValue)
            {
                var commentsByUserId = await _commentService.GetCommentsByUserIdAsync(userId.Value);
                return BaseHttpResponse<List<GetCommentResponse>>.Success(commentsByUserId);
            }

            // 如果 postId 和 userId 都没有提供
            return BaseHttpResponse<List<GetCommentResponse>>.Fail(400, "参数 postId 和 userId 至少需要提供一个");
        }
        catch (Exception ex)
        {
            // 记录日志（推荐注入 ILogger）
            return BaseHttpResponse<List<GetCommentResponse>>.Fail(500, $"服务器内部错误：{ex.Message}");
        }
    }

    [HttpGet("comment")]
    public async Task<BaseHttpResponse<List<GetCommentResponse>>> GetComment([FromQuery] string ids,
        [FromQuery] CommentRequest.CommentType type)
    {
        if (string.IsNullOrEmpty(ids))
        {
            return BaseHttpResponse<List<GetCommentResponse>>.Fail(400, "Ids parameter is required.");
        }

        try
        {
            // 将逗号分隔的字符串转换为 long 列表
            var idList = ids.Split(',')
                .Select(id => long.Parse(id.Trim()))
                .ToList();

            var comments = await _commentService.GetCommentsByIdsAsync(idList, type);

            return BaseHttpResponse<List<GetCommentResponse>>.Success(comments);
        }
        catch (FormatException)
        {
            return BaseHttpResponse<List<GetCommentResponse>>.Fail(400, "Invalid id format.");
        }
        catch (Exception ex)
        {
            // 记录异常日志
            return BaseHttpResponse<List<GetCommentResponse>>.Fail(500, "An error occurred.");
        }
    }

    [HttpPost("{post_id}/share")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<BaseHttpResponse<object?>> SharePost([FromRoute] long post_id)
    {
        try
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return BaseHttpResponse<object?>.Fail(401, "未认证的用户");
            }

            long userId = long.Parse(userIdClaim.Value);

            await _shareService.SharePostAsync(userId, post_id);

            return BaseHttpResponse<object?>.Success(null, "分享成功");
        }
        catch (Exception ex)
        {
            return BaseHttpResponse<object?>.Fail(500, "分享失败：" + ex.Message);
        }
    }

    [HttpPost("{postId:long}/views/increment")]
    [AllowAnonymous]
    public async Task<BaseHttpResponse<object>> IncrementViews([FromRoute] long postId, CancellationToken ct)
    {
        try
        {
            var views = await _postService.IncrementViewsAsync(postId, ct);
            if (views is null)
                return BaseHttpResponse<object>.Fail(404, "Post not found");

            return BaseHttpResponse<object>.Success(new { postId, views });
        }
        catch (Exception ex)
        {
            return BaseHttpResponse<object>.Fail(500, "服务器内部错误：" + ex.Message);
        }
    }

    [HttpPost("like")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> LikePost([FromBody] LikeRequest request)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return (IActionResult)BaseHttpResponse<object?>.Fail(401, "未认证的用户");
        }

        var userId = long.Parse(userIdClaim.Value);
        await _likeService.ToggleLikeAsync(userId, request);
        return Ok(new { code = 200, msg = (string)null, data = (object)null });
    }

    [HttpPost("CheckLike")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<BaseHttpResponse<CheckLikeResponse>> CheckLike([FromBody] CheckLikeRequest request)
    {
        try
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return BaseHttpResponse<CheckLikeResponse>.Fail(401, "未认证的用户");
            }

            var userId = long.Parse(userIdClaim.Value);
            var isLiked = await _likeService.GetLikeStatusAsync(userId, request.Type, request.TargetId);
            var response = new { isLiked };
            return BaseHttpResponse<CheckLikeResponse>.Success(new CheckLikeResponse { IsLiked = isLiked });
        }
        catch (Exception ex)
        {
            return BaseHttpResponse<CheckLikeResponse>.Fail(500, "服务器内部错误：" + ex.Message);
        }
    }
    
    [HttpGet("mine")]
    [Authorize(AuthenticationSchemes = "Bearer")] // 需要登录；未登录将被框架拦截为 401
    public async Task<BaseHttpResponse<List<PostResponse>>> GetMyPosts(CancellationToken ct)
    {
        try
        {
            // ✅ 参考 DeletePost 的写法：从 JWT 取当前用户 Id
            // 常见 Claim：NameIdentifier / "userId" / "uid" / sub（按你们 DeletePost 的实现来）
            // 从JWT token中提取用户ID（你可能需要配置Claims）
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return BaseHttpResponse<List<PostResponse>>.Fail(401, "未认证的用户");
            }

            var userId = long.Parse(userIdClaim.Value);

            var posts = await _postService.GetMyPostsAsync(userId, ct);

            var data = posts.Select(p => new PostResponse
            {
                PostId   = p.PostId,
                UserId   = p.UserId ?? 0,
                Title    = p.Title ?? "",
                Tags     = p.TagNames ?? new List<string>(),
                CreatedAt= p.CreatedAt ?? DateTime.MinValue,
                IsHidden = p.IsHidden,
                Views    = p.Views ?? 0,
                Likes    = p.Likes ?? 0,
                CircleId = p.CircleId
            }).ToList();

            return BaseHttpResponse<List<PostResponse>>.Success(data);
        }
        catch (Exception ex)
        {
            return BaseHttpResponse<List<PostResponse>>.Fail(500, "服务器内部错误：" + ex.Message);
        }
    }
    
    [HttpPut("{postId:long}/hidden")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<BaseHttpResponse<object>> SetHidden(
        long postId,
        [FromQuery] bool next,
        CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return BaseHttpResponse<object>.Fail(401, "未认证的用户");

            if (!long.TryParse(userIdClaim.Value, out var userId) || userId <= 0)
                return BaseHttpResponse<object>.Fail(401, "未认证的用户");

            var (exists, owned, updated, currentHidden) =
                await _postService.SetHiddenAsync(postId, userId, next, ct);

            if (!exists) return BaseHttpResponse<object>.Fail(404, "帖子不存在");
            if (!owned)  return BaseHttpResponse<object>.Fail(403, "无权限修改他人帖子");

            return BaseHttpResponse<object>.Success(new {
                postId,
                isHidden = currentHidden != 0,
                updated  // true=这次确实写库；false=本来就是该状态（幂等）
            });
        }
        catch (Exception ex)
        {
            return BaseHttpResponse<object>.Fail(500, "服务器内部错误：" + ex.Message);
        }
    }
    
    [HttpPut]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<BaseHttpResponse<PostResponse>> Update(
        [FromBody] PostUpdateRequest req,
        CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return BaseHttpResponse<PostResponse>.Fail(401, "未认证的用户");
            }
            var userId = long.Parse(userIdClaim.Value);

            // 最小校验（也可用 FluentValidation）
            if (string.IsNullOrWhiteSpace(req.Title))
                return BaseHttpResponse<PostResponse>.Fail(400, "Title is required");

            var result = await _postService.UpdatePostAsync(req, userId, ct);

            return result.Code switch
            {
                200 => BaseHttpResponse<PostResponse>.Success(result.Data!),
                403 => BaseHttpResponse<PostResponse>.Fail(403, "Forbidden"),
                404 => BaseHttpResponse<PostResponse>.Fail(404, "Post not found"),
                409 => BaseHttpResponse<PostResponse>.Fail(409, result.Error ?? "Conflict"),
                _   => BaseHttpResponse<PostResponse>.Fail(500, result.Error ?? "Server error")
            };
        }
        catch (Exception ex)
        {
            return BaseHttpResponse<PostResponse>.Fail(500, "服务器内部错误：" + ex.Message);
        }
    }
}