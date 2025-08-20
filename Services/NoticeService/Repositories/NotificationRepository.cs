using Microsoft.EntityFrameworkCore;
using NoticeService.Data;
using NoticeService.Models;
using NoticeService.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoticeService.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NoticeDbContext _context;

        public NotificationRepository(NoticeDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, int>> GetUnreadCountsAsync(int userId)
        {
            var replyUnread = await _context.Replies
                .CountAsync(r => r.TargetUserId == userId && !r.IsRead && !r.IsDeleted);
            var likeUnread = await _context.Likes
                .CountAsync(l => l.TargetUserId == userId && !l.IsRead);
            var repostUnread = await _context.Reposts
                .CountAsync(rp => rp.TargetUserId == userId && !rp.IsRead);
            var mentionUnread = await _context.Mentions
                .CountAsync(m => m.TargetUserId == userId && !m.IsRead);

            return new Dictionary<string, int>
            {
                { "reply", replyUnread },
                { "like", likeUnread },
                { "repost", repostUnread },
                { "mention", mentionUnread }
            };
        }

        public async Task MarkAsReadAsync(int userId, string type)
        {
            switch (type.ToLower())
            {
                case "reply":
                    var replies = await _context.Replies
                        .Where(r => r.TargetUserId == userId && !r.IsRead && !r.IsDeleted)
                        .ToListAsync();
                    replies.ForEach(r => r.IsRead = true);
                    break;
                case "like":
                    var likes = await _context.Likes
                        .Where(l => l.TargetUserId == userId && !l.IsRead)
                        .ToListAsync();
                    likes.ForEach(l => l.IsRead = true);
                    break;
                case "repost":
                    var reposts = await _context.Reposts
                        .Where(rp => rp.TargetUserId == userId && !rp.IsRead)
                        .ToListAsync();
                    reposts.ForEach(rp => rp.IsRead = true);
                    break;
                case "mention":
                    var mentions = await _context.Mentions
                        .Where(m => m.TargetUserId == userId && !m.IsRead)
                        .ToListAsync();
                    mentions.ForEach(m => m.IsRead = true);
                    break;
                default:
                    throw new ArgumentException("无效的通知类型");
            }
            await _context.SaveChangesAsync();
        }

        public async Task<(List<Like> Unread, (List<Like> Items, int Total) Read)> GetLikesAsync(int userId, int page, int pageSize)
        {
            // 未读点赞：全量获取，按 target_type 和 target_id 分组
            var unreadQuery = _context.Likes
                .Where(l => l.TargetUserId == userId && !l.IsRead);

            var unreadLikes = await unreadQuery
                .GroupBy(l => new { l.TargetType, l.TargetId })
                .Select(g => new Like
                {
                    TargetType = g.Key.TargetType,
                    TargetId = g.Key.TargetId,
                    CreatedAt = g.Max(l => l.CreatedAt), // LastLikedAt
                    UserId = g.Count(), // LikeCount
                    LikerIds = g.Select(l => l.UserId).Take(10).ToList() // 点赞者 ID 摘要（最多 10 个）
                })
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            // 已读点赞：分页获取，分组
            var readQuery = _context.Likes
                .Where(l => l.TargetUserId == userId && l.IsRead);

            var readTotal = await readQuery
                .GroupBy(l => new { l.TargetType, l.TargetId })
                .CountAsync();

            var readLikes = await readQuery
                .GroupBy(l => new { l.TargetType, l.TargetId })
                .Select(g => new Like
                {
                    TargetType = g.Key.TargetType,
                    TargetId = g.Key.TargetId,
                    CreatedAt = g.Max(l => l.CreatedAt), // LastLikedAt
                    UserId = g.Count(), // LikeCount
                    LikerIds = g.Select(l => l.UserId).Take(10).ToList() // 点赞者 ID 摘要
                })
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (unreadLikes, (readLikes, readTotal));
        }

        public async Task<List<Reply>> GetRepliesAsync(int userId, int page, int pageSize, bool unreadOnly)
        {
            var query = _context.Replies
                .Where(r => r.TargetUserId == userId && !r.IsDeleted);
            if (unreadOnly)
                query = query.Where(r => !r.IsRead);

            return await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Repost>> GetRepostsAsync(int userId, int page, int pageSize, bool unreadOnly)
        {
            var query = _context.Reposts
                .Where(rp => rp.TargetUserId == userId);
            if (unreadOnly)
                query = query.Where(rp => !rp.IsRead);

            return await query
                .OrderByDescending(rp => rp.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Mention>> GetMentionsAsync(int userId, int page, int pageSize, bool unreadOnly)
        {
            var query = _context.Mentions
                .Where(m => m.TargetUserId == userId);
            if (unreadOnly)
                query = query.Where(m => !m.IsRead);

            return await query
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<(List<int> Items, int Total)> GetTargetLikersAsync(int userId, string targetType, int targetId, int page, int pageSize)
        {
            var query = _context.Likes
                .Where(l => l.TargetUserId == userId && l.TargetType == targetType && l.TargetId == targetId);

            var total = await query.CountAsync();

            var likerIds = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => l.UserId)
                .ToListAsync();

            return (likerIds, total);
        }
    }
}