using AutoMapper;
using NoticeService.DTOs;
using NoticeService.Models;
using NoticeService.Repositories;
using NoticeService.Services;
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
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<NotificationSummaryDto> GetNotificationSummaryAsync(int userId)
        {
            var counts = await _repository.GetUnreadCountsAsync(userId);
            return new NotificationSummaryDto
            {
                TotalUnread = counts.Values.Sum(),
                UnreadByType = counts
            };
        }

        public async Task MarkAsReadAsync(int userId, string type)
        {
            await _repository.MarkAsReadAsync(userId, type);
        }

        public async Task<LikeResponseDto> GetLikesAsync(int userId, int page, int pageSize)
        {
            var (unreadLikes, (readLikes, readTotal)) = await _repository.GetLikesAsync(userId, page, pageSize);
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

        public async Task<List<ReplyNotificationDto>> GetRepliesAsync(int userId, int page, int pageSize, bool unreadOnly)
        {
            var replies = await _repository.GetRepliesAsync(userId, page, pageSize, unreadOnly);
            return _mapper.Map<List<ReplyNotificationDto>>(replies);
        }

        public async Task<List<RepostNotificationDto>> GetRepostsAsync(int userId, int page, int pageSize, bool unreadOnly)
        {
            var reposts = await _repository.GetRepostsAsync(userId, page, pageSize, unreadOnly);
            return _mapper.Map<List<RepostNotificationDto>>(reposts);
        }

        public async Task<List<MentionNotificationDto>> GetMentionsAsync(int userId, int page, int pageSize, bool unreadOnly)
        {
            var mentions = await _repository.GetMentionsAsync(userId, page, pageSize, unreadOnly);
            return _mapper.Map<List<MentionNotificationDto>>(mentions);
        }

        public async Task<TargetLikerDto> GetTargetLikersAsync(int userId, string targetType, int targetId, int page, int pageSize)
        {
            var (likerIds, total) = await _repository.GetTargetLikersAsync(userId, targetType, targetId, page, pageSize);
            return new TargetLikerDto
            {
                Total = total,
                Items = likerIds
            };
        }
    }
}