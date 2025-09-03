using DTOs;
using PostService.DTOs;
using Models;
using PostService.Repositories;

namespace PostService.Services;

public interface ICommentService
{
    Task<BaseHttpResponse<CommentResponse>> AddCommentOrReplyAsync(long userId, CommentRequest request);
    Task<BaseHttpResponse<object?>> DeleteCommentOrReplyAsync(long userId, long commentId, CommentRequest.CommentType type);
    Task<BaseHttpResponse<List<GetCommentResponse>>> GetCommentsByPostIdAsync(long postId);
    Task<List<GetCommentResponse>> GetCommentsByUserIdAsync(long userId);
}

public class CommentService : ICommentService
{
    private readonly ICommentRepository _repo;

    public CommentService(ICommentRepository repo)
    {
        _repo = repo;
    }

    public async Task<BaseHttpResponse<CommentResponse>> AddCommentOrReplyAsync(long userId, CommentRequest request)
    {
        // 1. 校验用户是否存在
        var userExists = await _repo.UserExistsAsync(userId);
        if (!userExists)
        {
            return BaseHttpResponse<CommentResponse>.Fail(401,"User not found");
        }

        if (request.Type == CommentRequest.CommentType.Comment)
        {
            // 2. 校验帖子是否存在
            var postExists = await _repo.PostExistsAsync(request.TargetId);
            if (!postExists)
            {
                return BaseHttpResponse<CommentResponse>.Fail(404, "Post not found" );
            }

            var comment = new Comments
            {
                PostId = request.TargetId,
                UserId = userId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                Likes = 0,
                IsDeleted = 0
            };

            var saved = await _repo.AddCommentAsync(comment);

            return BaseHttpResponse<CommentResponse>.Success(new CommentResponse
            {
                Type = request.Type,
                Id = saved.CommentId,
                CreatedAt = saved.CreatedAt
            });
        }
        else
        {
            // 3. 校验评论是否存在
            var commentExists = await _repo.CommentExistsAsync(request.TargetId);
            if (!commentExists)
            {
                return BaseHttpResponse<CommentResponse>.Fail(404, "Comment not found");
            }

            var reply = new Reply
            {
                CommentId = request.TargetId,
                UserId = userId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = 0
            };

            var saved = await _repo.AddReplyAsync(reply);

            return BaseHttpResponse<CommentResponse>.Success(new CommentResponse
            {
                Type = request.Type,
                Id = saved.ReplyId,
                CreatedAt = saved.CreatedAt
            });
        }
    }

    public async Task<BaseHttpResponse<object?>> DeleteCommentOrReplyAsync(long userId, long id, CommentRequest.CommentType type)
    {
        switch (type)
        {
            case CommentRequest.CommentType.Comment:
                var comment = await _repo.GetCommentByIdAsync(id);
                if (comment == null)
                    return BaseHttpResponse<object?>.Fail(404, "评论不存在");

                if (comment.UserId != userId)
                    return BaseHttpResponse<object?>.Fail(403, "没有权限删除该评论");

                comment.IsDeleted = 1;
                await _repo.UpdateCommentAsync(comment);
                return BaseHttpResponse<object?>.Success(null, "删除评论成功");

            case CommentRequest.CommentType.Reply:
                var reply = await _repo.GetReplyByIdAsync(id);
                if (reply == null)
                    return BaseHttpResponse<object?>.Fail(404, "回复不存在");

                if (reply.UserId != userId)
                    return BaseHttpResponse<object?>.Fail(403, "没有权限删除该回复");

                reply.IsDeleted = 1;
                await _repo.UpdateReplyAsync(reply);
                return BaseHttpResponse<object?>.Success(null, "删除回复成功");

            default:
                return BaseHttpResponse<object?>.Fail(400, "未知类型");
        }
    }

    public async Task<BaseHttpResponse<List<GetCommentResponse>>> GetCommentsByPostIdAsync(long postId)
    {
        var comments = await _repo.GetCommentsByPostIdAsync(postId);

        var result = new List<GetCommentResponse>();

        foreach (var c in comments)
        {
            // 直接使用已加载的导航属性，无需进行额外的数据库查询
            result.Add(new GetCommentResponse
            {
                Type = CommentRequest.CommentType.Comment,
                Id = c.CommentId,
                TargetId = c.PostId,
                UserId = c.UserId,
                UserName = c.User?.Username, // 安全访问已加载的 User
                AvatarUrl = c.User?.UserProfile?.AvatarUrl, // 安全访问已加载的 UserProfile
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                Likes = c.Likes
            });

            // 同样，直接使用已加载的 Replies 集合
            foreach (var r in c.Replies)
            {
                result.Add(new GetCommentResponse
                {
                    Type = CommentRequest.CommentType.Reply,
                    Id = r.ReplyId,
                    TargetId = r.CommentId,
                    UserId = r.UserId,
                    UserName = r.User?.Username, // 安全访问已加载的 User
                    AvatarUrl = r.User?.UserProfile?.AvatarUrl, // 安全访问已加载的 UserProfile
                    Content = r.Content,
                    CreatedAt = r.CreatedAt,
                    Likes = 0 // 回复没有点赞字段，这里可以根据你的业务逻辑设定
                });
            }
        }

        return BaseHttpResponse<List<GetCommentResponse>>.Success(result);
    }
    
    public async Task<List<GetCommentResponse>> GetCommentsByUserIdAsync(long userId)
    {
        // 1. 获取用户发表的评论
        var comments = await _repo.GetCommentsByUserIdAsync(userId);
        
        // 2. 获取用户发表的回复
        var replies = await _repo.GetRepliesByUserIdAsync(userId);

        var result = new List<GetCommentResponse>();

        // 3. 将评论映射到响应模型
        foreach (var c in comments)
        {
            result.Add(new GetCommentResponse
            {
                Type = CommentRequest.CommentType.Comment,
                Id = c.CommentId,
                TargetId = c.PostId,
                PostTitle = c.Post?.Title, // 从导航属性获取帖子标题
                UserId = c.UserId,
                UserName = c.User?.Username,
                AvatarUrl = c.User?.UserProfile?.AvatarUrl,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                Likes = c.Likes
            });
        }

        // 4. 将回复映射到响应模型
        foreach (var r in replies)
        {
            result.Add(new GetCommentResponse
            {
                Type = CommentRequest.CommentType.Reply,
                Id = r.ReplyId,
                TargetId = r.Comment?.PostId,
                PostTitle = r.Comment?.Post?.Title, // 通过评论导航属性获取帖子标题
                UserId = r.UserId,
                UserName = r.User?.Username,
                AvatarUrl = r.User?.UserProfile?.AvatarUrl,
                Content = r.Content,
                CreatedAt = r.CreatedAt,
                Likes = 0 // 回复没有点赞字段
            });
        }
        
        // 5. 按时间排序所有评论和回复，并返回
        return result.OrderByDescending(x => x.CreatedAt).ToList();
    }
}