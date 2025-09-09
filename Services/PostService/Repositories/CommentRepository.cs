using Microsoft.EntityFrameworkCore;
using Models;
using PostService.Data;
using PostService.Models;

namespace PostService.Repositories
{
    public interface ICommentRepository
    {
        Task<Comments?> GetCommentByIdAsync(long commentId);
        Task<Reply?> GetReplyByIdAsync(long replyId);
        Task<Comments> AddCommentAsync(Comments comment);
        Task<Reply> AddReplyAsync(Reply reply);
        Task UpdateCommentAsync(Comments comment);
        Task UpdateReplyAsync(Reply reply);
        Task<long?> GetUserIdByPostIdAsync(long postId);
        Task<long?> GetUserIdByCommentIdAsync(long commentId);
        Task<List<Comments>> GetCommentsByPostIdAsync(long postId);
        Task<List<Comments>> GetCommentsByUserIdAsync(long userId);
        Task<List<Reply>> GetRepliesByUserIdAsync(long userId);
        Task<bool> UserExistsAsync(long userId);
        Task<bool> PostExistsAsync(long postId);
        Task<bool> CommentExistsAsync(long commentId);
        Task<List<Comments>> GetCommentsByIdsAsync(List<long> ids);
        Task<List<Reply>> GetRepliesByIdsAsync(List<long> ids);
    }

    public class CommentRepository : ICommentRepository
    {
        private readonly AppDbContext _db;

        public CommentRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Comments?> GetCommentByIdAsync(long commentId)
            => await _db.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId && c.IsDeleted == 0);

        public async Task<Reply?> GetReplyByIdAsync(long replyId)
            => await _db.Replies.FirstOrDefaultAsync(r => r.ReplyId == replyId && r.IsDeleted == 0);

        public async Task<Comments> AddCommentAsync(Comments comment)
        {
            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();
            return comment;
        }

        public async Task<Reply> AddReplyAsync(Reply reply)
        {
            _db.Replies.Add(reply);
            await _db.SaveChangesAsync();
            return reply;
        }

        public async Task UpdateCommentAsync(Comments comment)
        {
            _db.Comments.Update(comment);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateReplyAsync(Reply reply)
        {
            _db.Replies.Update(reply);
            await _db.SaveChangesAsync();
        }

        public async Task<long?> GetUserIdByPostIdAsync(long postId)
        {
            return await _db.Posts
                .Where(p => p.PostId == postId)
                .Select(p => p.UserId)
                .FirstOrDefaultAsync();
        }

        public async Task<long?> GetUserIdByCommentIdAsync(long commentId)
        {
            return await _db.Comments
                .Where(c => c.CommentId == commentId)
                .Select(c => c.UserId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Comments>> GetCommentsByPostIdAsync(long postId)
        {
            return await _db.Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.User) // 加载评论的用户信息
                    .ThenInclude(u => u.UserProfile) // 进一步加载用户的个人档案
                .Include(c => c.Replies) // 加载评论的所有回复
                    .ThenInclude(r => r.User) // 进一步加载回复的用户信息
                        .ThenInclude(u => u.UserProfile) // 进一步加载用户的个人档案
                .ToListAsync();
        }
        
        // 新增方法：按用户ID查询所有评论
        public async Task<List<Comments>> GetCommentsByUserIdAsync(long userId)
        {
            return await _db.Comments
                .Where(c => c.UserId == userId)
                .Include(c => c.User)
                    .ThenInclude(u => u.UserProfile)
                .Include(c => c.Post) // 加载评论所属的帖子信息
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        // 新增方法：按用户ID查询所有回复
        public async Task<List<Reply>> GetRepliesByUserIdAsync(long userId)
        {
            return await _db.Replies
                .Where(r => r.UserId == userId)
                .Include(r => r.User)
                    .ThenInclude(u => u.UserProfile)
                .Include(r => r.Comment) // 加载回复所属的评论
                    .ThenInclude(c => c.Post) // 通过评论加载帖子信息
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<bool> UserExistsAsync(long userId)
        {
            return await _db.Users.CountAsync(u => u.UserId == userId) > 0;
        }

        public async Task<bool> PostExistsAsync(long postId)
        {
            return await _db.Posts.CountAsync(p => p.PostId == postId && p.IsDeleted == 0) > 0;
        }

        public async Task<bool> CommentExistsAsync(long commentId)
        {
            return await _db.Comments.CountAsync(c => c.CommentId == commentId && c.IsDeleted == 0) > 0;
        }

        public async Task<List<Comments>> GetCommentsByIdsAsync(List<long> ids)
        {
            return await _db.Comments
                .Include(c => c.User)
                .Include(c => c.Post)
                .Where(c => ids.Contains(c.CommentId) && c.IsDeleted == 0)
                .ToListAsync();
        }

        public async Task<List<Reply>> GetRepliesByIdsAsync(List<long> ids)
        {
            return await _db.Replies
                .Include(r => r.User)
                .Where(r => ids.Contains(r.ReplyId) && r.IsDeleted == 0)
                .ToListAsync();
        }
    }
}