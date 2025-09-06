using Microsoft.EntityFrameworkCore;
using NoticeService.Data;
using NoticeService.Models;
using NoticeService.Repositories;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace NoticeService.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NoticeDbContext _context;

        public NotificationRepository(NoticeDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Dictionary<string, int>> GetUnreadCountsAsync(int userId, IDatabase redis)
        {
            if (redis == null) throw new ArgumentNullException(nameof(redis));
            var types = new[] { "reply", "like", "repost", "mention" };
            var counts = new Dictionary<string, int>();

            foreach (var type in types)
            {
                var key = $"unread-notice:{userId}:{type}";
                Console.WriteLine($"Calling ListLengthAsync for key: {key}");
                var count = (int)(await redis.ListLengthAsync(key, CommandFlags.None));
                counts[type] = count;
            }

            return counts;
        }

        public async Task MarkAsReadAsync(int userId, string type, IDatabase redis)
        {
            if (redis == null) throw new ArgumentNullException(nameof(redis));
            if (string.IsNullOrEmpty(type)) throw new ArgumentException("Type cannot be null or empty", nameof(type));
            var key = $"unread-notice:{userId}:{type.ToLower()}";
            Console.WriteLine($"Calling KeyDeleteAsync for key: {key}");
            await redis.KeyDeleteAsync(key, CommandFlags.None);
        }

        public async Task<(List<Like> Unread, (List<Like> Items, int Total) Read)> GetLikesAsync(int userId, int page, int pageSize, IDatabase redis)
        {
            if (redis == null) throw new ArgumentNullException(nameof(redis));
            var likeKey = $"unread-notice:{userId}:like";
            Console.WriteLine($"Calling ListRangeAsync for key: {likeKey}");
            var unreadLikeIds = (await redis.ListRangeAsync(likeKey, 0, -1, CommandFlags.None))
                .Select(id => JsonSerializer.Deserialize<(int UserId, string TargetType, int TargetId)>(id))
                .ToList();

            var unreadLikes = new List<Like>();
            if (unreadLikeIds.Any())
            {
                var unreadQuery = _context.Likes
                    .Where(l => unreadLikeIds.Any(id => id.UserId == l.UserId && id.TargetType == l.TargetType && id.TargetId == l.TargetId));

                unreadLikes = await unreadQuery
                    .GroupBy(l => new { l.TargetType, l.TargetId })
                    .Select(g => new Like
                    {
                        TargetType = g.Key.TargetType,
                        TargetId = g.Key.TargetId,
                        CreatedAt = g.Max(l => l.CreatedAt),
                        UserId = g.Count(),
                        LikerIds = g.Select(l => l.UserId).Take(10).ToList()
                    })
                    .OrderByDescending(l => l.CreatedAt)
                    .ToListAsync();
            }

            var allLikesQuery = _context.Likes
                .Where(l => !unreadLikeIds.Any(id => id.UserId == l.UserId && id.TargetType == l.TargetType && id.TargetId == l.TargetId));

            var readTotal = await allLikesQuery
                .GroupBy(l => new { l.TargetType, l.TargetId })
                .CountAsync();

            var readLikes = await allLikesQuery
                .GroupBy(l => new { l.TargetType, l.TargetId })
                .Select(g => new Like
                {
                    TargetType = g.Key.TargetType,
                    TargetId = g.Key.TargetId,
                    CreatedAt = g.Max(l => l.CreatedAt),
                    UserId = g.Count(),
                    LikerIds = g.Select(l => l.UserId).Take(10).ToList()
                })
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (unreadLikes, (readLikes, readTotal));
        }

        public async Task<List<Reply>> GetRepliesAsync(int userId, int page, int pageSize, bool unreadOnly, IDatabase redis)
        {
            if (redis == null) throw new ArgumentNullException(nameof(redis));
            var replyKey = $"unread-notice:{userId}:comment";
            Console.WriteLine($"Calling ListRangeAsync for key: {replyKey}");
            var unreadReplyIds = (await redis.ListRangeAsync(replyKey, 0, -1, CommandFlags.None))
                .Select(id => int.Parse(id)).ToList();

            var query = _context.Replies
                .Where(r => r.TargetUserId == userId && !r.IsDeleted);

            if (unreadOnly)
                query = query.Where(r => unreadReplyIds.Contains(r.ReplyId));
            else
                query = query.Where(r => !unreadReplyIds.Contains(r.ReplyId) || unreadReplyIds.Contains(r.ReplyId));

            return await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Repost>> GetRepostsAsync(int userId, int page, int pageSize, bool unreadOnly, IDatabase redis)
        {
            if (redis == null) throw new ArgumentNullException(nameof(redis));
            var repostKey = $"unread-notice:{userId}:repost";
            Console.WriteLine($"Calling ListRangeAsync for key: {repostKey}");
            var unreadRepostIds = (await redis.ListRangeAsync(repostKey, 0, -1, CommandFlags.None))
                .Select(id => int.Parse(id)).ToList();

            var query = _context.Reposts
                .Where(rp => rp.TargetUserId == userId);

            if (unreadOnly)
                query = query.Where(rp => unreadRepostIds.Contains(rp.RepostId));
            else
                query = query.Where(rp => !unreadRepostIds.Contains(rp.RepostId) || unreadRepostIds.Contains(rp.RepostId));

            return await query
                .OrderByDescending(rp => rp.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Mention>> GetMentionsAsync(int userId, int page, int pageSize, bool unreadOnly, IDatabase redis)
        {
            if (redis == null) throw new ArgumentNullException(nameof(redis));
            var mentionKey = $"unread-notice:{userId}:mention";
            Console.WriteLine($"Calling ListRangeAsync for key: {mentionKey}");
            var unreadMentionIds = (await redis.ListRangeAsync(mentionKey, 0, -1, CommandFlags.None))
                .Select(id => JsonSerializer.Deserialize<(int UserId, string TargetType, int TargetId)>(id))
                .ToList();

            var query = _context.Mentions
                .Where(m => m.TargetUserId == userId);

            if (unreadOnly)
                query = query.Where(m => unreadMentionIds.Any(id => id.UserId == m.UserId && id.TargetType == m.TargetType && id.TargetId == m.TargetId));
            else
                query = query.Where(m => !unreadMentionIds.Any(id => id.UserId == m.UserId && id.TargetType == m.TargetType && id.TargetId == m.TargetId) ||
                    unreadMentionIds.Any(id => id.UserId == m.UserId && id.TargetType == m.TargetType && id.TargetId == m.TargetId));

            return await query
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<(List<int> Items, int Total)> GetTargetLikersAsync(int userId, string targetType, int targetId, int page, int pageSize)
        {
            var query = _context.Likes
                .Where(l => l.TargetType == targetType && l.TargetId == targetId);

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