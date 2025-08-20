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
        Task<List<Comments>> GetCommentsByPostIdAsync(long postId);
        Task<List<Reply>> GetRepliesByCommentIdsAsync(IEnumerable<long> commentIds);
        Task<User?> GetUserByIdAsync(long userId);
        Task<UserProfile?> GetUserProfileByIdAsync(long userId);
        Task<bool> UserExistsAsync(long userId);
        Task<bool> PostExistsAsync(long postId);
        Task<bool> CommentExistsAsync(long commentId);
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

        public async Task<List<Comments>> GetCommentsByPostIdAsync(long postId)
            => await _db.Comments.Where(c => c.PostId == postId && c.IsDeleted == 0).ToListAsync();

        public async Task<List<Reply>> GetRepliesByCommentIdsAsync(IEnumerable<long> commentIds)
            => await _db.Replies.Where(r => commentIds.Contains(r.CommentId) && r.IsDeleted == 0).ToListAsync();

        public async Task<User?> GetUserByIdAsync(long userId)
            => await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);

        public async Task<UserProfile?> GetUserProfileByIdAsync(long userId)
            => await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        
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
    }
}