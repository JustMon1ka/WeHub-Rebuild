using NoticeService.DTOs;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NoticeService.Services
{
    public interface INotificationService
    {
        Task<NotificationSummaryDto> GetNotificationSummaryAsync(int userId, IDatabase redis);
        Task MarkAsReadAsync(int userId, string type, IDatabase redis);
        Task<LikeResponseDto> GetLikesAsync(int userId, int page, int pageSize, IDatabase redis);
        Task<List<ReplyNotificationDto>> GetRepliesAsync(int userId, int page, int pageSize, bool unreadOnly, IDatabase redis);
        Task<List<RepostNotificationDto>> GetRepostsAsync(int userId, int page, int pageSize, bool unreadOnly, IDatabase redis);
        Task<List<MentionNotificationDto>> GetMentionsAsync(int userId, int page, int pageSize, bool unreadOnly, IDatabase redis);
        Task<TargetLikerDto> GetTargetLikersAsync(int userId, string targetType, int targetId, int page, int pageSize);
        Task<List<CommentNotificationDto>> GetCommentsAsync(int userId, int page, int pageSize, bool unreadOnly, IDatabase redis);
        Task CreateLikeNotificationAsync(CreateLikeNotificationDto dto, IDatabase redis);
    }
}