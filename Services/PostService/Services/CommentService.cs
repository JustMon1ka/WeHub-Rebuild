using DTOs;
using PostService.DTOs;
using PostService.Models;
using PostService.Repositories;

namespace PostService.Services;

public interface ICommentService
{
    Task<BaseHttpResponse<CommentResponse>> AddCommentOrReplyAsync(long userId, CommentRequest request);
    Task<BaseHttpResponse<object?>> DeleteCommentOrReplyAsync(long userId, long commentId, CommentRequest.CommentType type);
    Task<BaseHttpResponse<List<GetCommentResponse>>> GetCommentsAsync(long postId);
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

    public async Task<BaseHttpResponse<List<GetCommentResponse>>> GetCommentsAsync(long postId)
    {
        var comments = await _repo.GetCommentsByPostIdAsync(postId);
        var replies = await _repo.GetRepliesByCommentIdsAsync(comments.Select(c => c.CommentId));

        var result = new List<GetCommentResponse>();

        foreach (var c in comments)
        {
            var user = await _repo.GetUserByIdAsync(c.UserId);
            var profile = await _repo.GetUserProfileByIdAsync(c.UserId);

            result.Add(new GetCommentResponse
            {
                Type = CommentRequest.CommentType.Comment,
                Id = c.CommentId,
                TargetId = c.PostId,
                UserId = c.UserId,
                UserName = user?.Username,
                AvatarUrl = profile?.AvatarUrl,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                Likes = c.Likes
            });

            var cReplies = replies.Where(r => r.CommentId == c.CommentId);
            foreach (var r in cReplies)
            {
                var rUser = await _repo.GetUserByIdAsync(r.UserId);
                var rProfile = await _repo.GetUserProfileByIdAsync(r.UserId);

                result.Add(new GetCommentResponse
                {
                    Type = CommentRequest.CommentType.Reply,
                    Id = r.ReplyId,
                    TargetId = r.CommentId,
                    UserId = r.UserId,
                    UserName = rUser?.Username,
                    AvatarUrl = rProfile?.AvatarUrl,
                    Content = r.Content,
                    CreatedAt = r.CreatedAt,
                    Likes = 0
                });
            }
        }

        return BaseHttpResponse<List<GetCommentResponse>>.Success(result);
    }
}