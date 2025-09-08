using AutoMapper;
using NoticeService.DTOs;
using NoticeService.Models;
using NoticeService.Repositories;
using NoticeService.Services;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NoticeService.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly IMapper _mapper;

        public NotificationService(INotificationRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<NotificationSummaryDto> GetNotificationSummaryAsync(int userId, IDatabase redis)
        {
            if (redis == null) throw new ArgumentNullException(nameof(redis));
            Console.WriteLine($"Calling GetUnreadCountsAsync for userId: {userId}");
            var counts = await _repository.GetUnreadCountsAsync(userId, redis);
            Console.WriteLine($"GetUnreadCountsAsync returned counts: {string.Join(", ", counts.Select(kv => $"{kv.Key}:{kv.Value}"))}");
            return new NotificationSummaryDto
            {
                TotalUnread = counts.Values.Sum(),
                UnreadByType = counts
            };
        }

        public async Task MarkAsReadAsync(int userId, string type, IDatabase redis)
        {
            if (redis == null) throw new ArgumentNullException(nameof(redis));
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Type cannot be null or empty", nameof(type));
            if (string.IsNullOrEmpty(type)) throw new ArgumentException("Type cannot be null or empty", nameof(type));
            Console.WriteLine($"Calling MarkAsReadAsync with userId: {userId}, type: {type}");
            await _repository.MarkAsReadAsync(userId, type, redis);
            Console.WriteLine("MarkAsReadAsync completed");
        }

        public async Task<LikeResponseDto> GetLikesAsync(int userId, int page, int pageSize, IDatabase redis)
        {
            if (redis == null) throw new ArgumentNullException(nameof(redis));
            Console.WriteLine($"Calling GetLikesAsync for userId: {userId}, page: {page}, pageSize: {pageSize}");
            var (unreadLikes, read) = await _repository.GetLikesAsync(userId, page, pageSize, redis);
            Console.WriteLine($"GetLikesAsync returned {unreadLikes.Count} unread likes, {read.Items.Count} read likes, total: {read.Total}");
            var (readLikes, readTotal) = read;
            var unreadDtos = _mapper.Map<List<LikeNotificationDto>>(unreadLikes);
            var readDtos = _mapper.Map<List<LikeNotificationDto>>(readLikes);

            return new LikeResponseDto
            {
                Unread = unreadDtos,
                Read = new ReadLikesDto
                {
                    Total = readTotal,
                    Items = readDtos
                }
            };
        }

        public async Task<List<ReplyNotificationDto>> GetRepliesAsync(int userId, int page, int pageSize, bool unreadOnly, IDatabase redis)
        {
            if (redis == null) throw new ArgumentNullException(nameof(redis));
            Console.WriteLine($"Calling GetRepliesAsync for userId: {userId}, page: {page}, pageSize: {pageSize}, unreadOnly: {unreadOnly}");
            var replies = await _repository.GetRepliesAsync(userId, page, pageSize, unreadOnly, redis);
            Console.WriteLine($"GetRepliesAsync returned {replies.Count} replies");
            if (unreadOnly && replies.Any())
            {
                var key = $"unread-notice:{userId}:comment";
                Console.WriteLine($"Auto marking replies as read, deleting key: {key}");
                await redis.KeyDeleteAsync(key, CommandFlags.None);
            }
            return _mapper.Map<List<ReplyNotificationDto>>(replies);
        }

        public async Task<List<RepostNotificationDto>> GetRepostsAsync(int userId, int page, int pageSize, bool unreadOnly, IDatabase redis)
        {
            if (redis == null) throw new ArgumentNullException(nameof(redis));
            Console.WriteLine($"Calling GetRepostsAsync for userId: {userId}, page: {page}, pageSize: {pageSize}, unreadOnly: {unreadOnly}");
            var reposts = await _repository.GetRepostsAsync(userId, page, pageSize, unreadOnly, redis);
            Console.WriteLine($"GetRepostsAsync returned {reposts.Count} reposts");
            return _mapper.Map<List<RepostNotificationDto>>(reposts);
        }

        public async Task<List<MentionNotificationDto>> GetMentionsAsync(int userId, int page, int pageSize, bool unreadOnly, IDatabase redis)
        {
            if (redis == null) throw new ArgumentNullException(nameof(redis));
            Console.WriteLine($"Calling GetMentionsAsync for userId: {userId}, page: {page}, pageSize: {pageSize}, unreadOnly: {unreadOnly}");
            var mentions = await _repository.GetMentionsAsync(userId, page, pageSize, unreadOnly, redis);
            Console.WriteLine($"GetMentionsAsync returned {mentions.Count} mentions");
            return _mapper.Map<List<MentionNotificationDto>>(mentions);
        }

        public async Task<TargetLikerDto> GetTargetLikersAsync(int userId, string targetType, int targetId, int page, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(targetType))
                throw new ArgumentException("Target type cannot be null or empty", nameof(targetType));
            Console.WriteLine($"Calling GetTargetLikersAsync for userId: {userId}, targetType: {targetType}, targetId: {targetId}");
            var (likerIds, total) = await _repository.GetTargetLikersAsync(userId, targetType, targetId, page, pageSize);
            Console.WriteLine($"GetTargetLikersAsync returned {likerIds.Count} liker IDs, total: {total}");
            return new TargetLikerDto
            {
                Total = total,
                Items = likerIds
            };
        }

        public async Task<List<CommentNotificationDto>> GetCommentsAsync(int userId, int page, int pageSize, bool unreadOnly, IDatabase redis)
        {
            if (redis == null) throw new ArgumentNullException(nameof(redis));
            Console.WriteLine($"Calling GetCommentsAsync for userId: {userId}, page: {page}, pageSize: {pageSize}, unreadOnly: {unreadOnly}");
            var comments = await _repository.GetCommentsAsync(userId, page, pageSize, unreadOnly, redis);
            Console.WriteLine($"GetCommentsAsync returned {comments.Count} comments");
            return _mapper.Map<List<CommentNotificationDto>>(comments);
        }
    }
}