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
            var types = new[] { "comment", "reply", "like", "repost", "mention" };
            var counts = new Dictionary<string, int>();

            foreach (var type in types)
            {
                var key = $"unread-notice:{userId}:{type}";
                Console.WriteLine($"Calling ListLengthAsync for key: {key}");
                var count = (int)(await redis.ListLengthAsync(key, CommandFlags.None));

                // 验证Redis数据的有效性
                if (count > 0 && type == "comment")
                {
                    var unreadIds = await redis.ListRangeAsync(key, 0, -1);
                    var commentIds = unreadIds.Where(id => !id.IsNull).Select(id => int.Parse(id!)).ToList();
                    var actualComments = await _context.Comments
                        .Where(c => c.TargetUserId == userId && commentIds.Contains(c.CommentId) && c.IsDeletedNumber == 0)
                        .CountAsync();

                    if (actualComments == 0)
                    {
                        Console.WriteLine($"[GetUnreadCountsAsync] Found orphaned Redis data for {type}, cleaning up...");
                        await redis.KeyDeleteAsync(key);
                        count = 0;
                    }
                }

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
            // 获取Redis中存储的未读通知ID列表（简单字符串格式）
            var unreadLikeIds = (await redis.ListRangeAsync(likeKey, 0, -1, CommandFlags.None))
                .Select(id => id.ToString())
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();

            Console.WriteLine($"Found {unreadLikeIds.Count} unread like IDs in Redis");

            // 先获取所有数据，然后在内存中处理
            var allLikes = await _context.Likes
                .Where(l => l.TargetUserId == userId)
                .ToListAsync();

            Console.WriteLine($"Found {allLikes.Count} total likes in database");

            var unreadLikes = new List<Like>();
            if (unreadLikeIds.Any())
            {
                // 将字符串ID转换为整数，用于匹配
                var unreadTargetIds = unreadLikeIds
                    .Where(id => int.TryParse(id, out _))
                    .Select(int.Parse)
                    .ToList();

                var unreadLikesData = allLikes.Where(l =>
                    unreadTargetIds.Contains(l.TargetId))
                    .ToList();

                Console.WriteLine($"Found {unreadLikesData.Count} unread likes data");

                unreadLikes = unreadLikesData
                    .GroupBy(l => new { l.TargetType, l.TargetId })
                    .Select(g => new Like
                    {
                        TargetType = g.Key.TargetType,
                        TargetId = g.Key.TargetId,
                        LikeTime = g.Max(l => l.LikeTime),
                        UserId = g.Count(),
                        LikerIds = g.Select(l => l.UserId).Take(10).ToList()
                    })
                    .OrderByDescending(l => l.LikeTime)
                    .ToList();
            }

            var readLikesData = allLikes.Where(l =>
                !unreadLikeIds.Contains(l.TargetId.ToString()))
                .ToList();

            var readTotal = readLikesData
                .GroupBy(l => new { l.TargetType, l.TargetId })
                .Count();

            var readLikes = readLikesData
                .GroupBy(l => new { l.TargetType, l.TargetId })
                .Select(g => new Like
                {
                    TargetType = g.Key.TargetType,
                    TargetId = g.Key.TargetId,
                    LikeTime = g.Max(l => l.LikeTime),
                    UserId = g.Count(),
                    LikerIds = g.Select(l => l.UserId).Take(10).ToList()
                })
                .OrderByDescending(l => l.LikeTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            Console.WriteLine($"Returning {unreadLikes.Count} unread likes and {readLikes.Count} read likes");
            return (unreadLikes, (readLikes, readTotal));
        }

        public async Task<List<Reply>> GetRepliesAsync(int userId, int page, int pageSize, bool unreadOnly, IDatabase redis)
        {
            if (redis == null) throw new ArgumentNullException(nameof(redis));
            var replyKey = $"unread-notice:{userId}:comment";
            Console.WriteLine($"Calling ListRangeAsync for key: {replyKey}");
            var unreadReplyIds = (await redis.ListRangeAsync(replyKey, 0, -1, CommandFlags.None))
                .Where(id => !id.IsNull)
                .Select(id => int.Parse(id!)).ToList();

            // 先获取所有数据，然后在内存中处理
            var allReplies = await _context.Replies
                .Where(r => r.TargetUserId == userId)
                .ToListAsync();

            // 在内存中过滤已删除的回复
            var filteredReplies = allReplies.Where(r => r.IsDeletedNumber == 0).ToList();

            List<Reply> result;
            if (unreadOnly)
                result = filteredReplies.Where(r => unreadReplyIds.Contains(r.ReplyId)).ToList();
            else
                result = filteredReplies.Where(r => !unreadReplyIds.Contains(r.ReplyId) || unreadReplyIds.Contains(r.ReplyId)).ToList();

            return result
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public async Task<List<Repost>> GetRepostsAsync(int userId, int page, int pageSize, bool unreadOnly, IDatabase redis)
        {
            if (redis == null) throw new ArgumentNullException(nameof(redis));
            var repostKey = $"unread-notice:{userId}:repost";
            Console.WriteLine($"Calling ListRangeAsync for key: {repostKey}");
            var unreadRepostIds = (await redis.ListRangeAsync(repostKey, 0, -1, CommandFlags.None))
                .Where(id => !id.IsNull)
                .Select(id => int.Parse(id!)).ToList();

            // 先获取所有数据，然后在内存中处理
            var allReposts = await _context.Reposts
                .Where(rp => rp.TargetUserId == userId)
                .ToListAsync();

            List<Repost> result;
            if (unreadOnly)
                result = allReposts.Where(rp => unreadRepostIds.Contains(rp.RepostId)).ToList();
            else
                result = allReposts.Where(rp => !unreadRepostIds.Contains(rp.RepostId) || unreadRepostIds.Contains(rp.RepostId)).ToList();

            return result
                .OrderByDescending(rp => rp.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public async Task<List<Mention>> GetMentionsAsync(int userId, int page, int pageSize, bool unreadOnly, IDatabase redis)
        {
            if (redis == null) throw new ArgumentNullException(nameof(redis));
            var mentionKey = $"unread-notice:{userId}:mention";
            Console.WriteLine($"Calling ListRangeAsync for key: {mentionKey}");
            var unreadMentionIds = (await redis.ListRangeAsync(mentionKey, 0, -1, CommandFlags.None))
                .Where(id => !id.IsNull)
                .Select(id => JsonSerializer.Deserialize<(int UserId, string TargetType, int TargetId)>(id.ToString()))
                .ToList();

            // 先获取所有数据，然后在内存中处理
            var allMentions = await _context.Mentions
                .Where(m => m.TargetUserId == userId)
                .ToListAsync();

            List<Mention> result;
            if (unreadOnly)
                result = allMentions.Where(m => unreadMentionIds.Any(id => id.UserId == m.UserId && id.TargetType == m.TargetType && id.TargetId == m.TargetId)).ToList();
            else
                result = allMentions.Where(m => !unreadMentionIds.Any(id => id.UserId == m.UserId && id.TargetType == m.TargetType && id.TargetId == m.TargetId) ||
                    unreadMentionIds.Any(id => id.UserId == m.UserId && id.TargetType == m.TargetType && id.TargetId == m.TargetId)).ToList();

            return result
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public async Task<(List<int> Items, int Total)> GetTargetLikersAsync(int userId, string targetType, int targetId, int page, int pageSize)
        {
            var query = _context.Likes
                .Where(l => l.TargetType == targetType && l.TargetId == targetId);

            var total = await query.CountAsync();

            var likerIds = await query
                .OrderByDescending(l => l.LikeTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => l.UserId)
                .ToListAsync();

            return (likerIds, total);
        }

        public async Task<List<Comment>> GetCommentsAsync(int userId, int page, int pageSize, bool unreadOnly, IDatabase redis)
        {
            var commentKey = $"unread-notice:{userId}:comment";
            var unreadCommentIds = (await redis.ListRangeAsync(commentKey, 0, -1, CommandFlags.None))
                .Where(id => !id.IsNull)
                .Select(id => int.Parse(id!)).ToList();

            Console.WriteLine($"[NotificationRepository] GetCommentsAsync: userId={userId}, unreadOnly={unreadOnly}");
            Console.WriteLine($"[NotificationRepository] Redis unread comment IDs: [{string.Join(", ", unreadCommentIds)}]");

            // 先获取所有数据，然后在内存中处理
            var allComments = await _context.Comments
                .Where(c => c.TargetUserId == userId)
                .ToListAsync();

            Console.WriteLine($"[NotificationRepository] Total comments in DB for user {userId}: {allComments.Count}");

            // 在内存中过滤已删除的评论
            var filteredComments = allComments.Where(c => c.IsDeletedNumber == 0).ToList();

            Console.WriteLine($"[NotificationRepository] Filtered comments (not deleted): {filteredComments.Count}");

            List<Comment> result;
            if (unreadOnly)
            {
                result = filteredComments.Where(c => unreadCommentIds.Contains(c.CommentId)).ToList();
                Console.WriteLine($"[NotificationRepository] Unread comments: {result.Count}");
            }
            else
            {
                // 修复逻辑：返回所有评论，不管是否在未读列表中
                result = filteredComments.ToList();
                Console.WriteLine($"[NotificationRepository] All comments: {result.Count}");
            }

            // 如果Redis中有未读ID但数据库中没有对应记录，清理Redis数据
            if (unreadCommentIds.Any() && !result.Any(c => unreadCommentIds.Contains(c.CommentId)))
            {
                Console.WriteLine($"[NotificationRepository] Found orphaned Redis data, cleaning up...");
                await redis.KeyDeleteAsync(commentKey);
                Console.WriteLine($"[NotificationRepository] Cleaned up Redis key: {commentKey}");
            }

            var pagedResult = result
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            Console.WriteLine($"[NotificationRepository] Final paged result: {pagedResult.Count} comments");
            return pagedResult;
        }

        public async Task CreateLikeAsync(Like like)
        {
            try
            {
                Console.WriteLine($"[NotificationRepository] Creating like: UserId={like.UserId}, TargetId={like.TargetId}, TargetType={like.TargetType}, TargetUserId={like.TargetUserId}");

                // 检查是否已经存在相同的点赞记录
                var existingLike = await _context.Likes
                    .FirstOrDefaultAsync(l => l.UserId == like.UserId && l.TargetId == like.TargetId && l.TargetType == like.TargetType);

                if (existingLike != null)
                {
                    Console.WriteLine($"[NotificationRepository] Like already exists, updating timestamp and target user");
                    existingLike.LikeTime = like.LikeTime;
                    existingLike.LikeType = like.LikeType;
                    existingLike.TargetUserId = like.TargetUserId; // 更新目标用户ID
                }
                else
                {
                    Console.WriteLine($"[NotificationRepository] Creating new like record");
                    _context.Likes.Add(like);
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"[NotificationRepository] Like saved successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationRepository] Error creating like: {ex.Message}");
                throw;
            }
        }
    }
}