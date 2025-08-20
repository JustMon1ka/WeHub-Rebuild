using NoticeService.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NoticeService.Services
{
    public interface INotificationService
    {
        Task<NotificationSummaryDto> GetNotificationSummaryAsync(int userId);
        Task MarkAsReadAsync(int userId, string type);
        Task<LikeResponseDto> GetLikesAsync(int userId, int page, int pageSize); // 更新返回类型
        Task<List<ReplyNotificationDto>> GetRepliesAsync(int userId, int page, int pageSize, bool unreadOnly);
        Task<List<RepostNotificationDto>> GetRepostsAsync(int userId, int page, int pageSize, bool unreadOnly);
        Task<List<MentionNotificationDto>> GetMentionsAsync(int userId, int page, int pageSize, bool unreadOnly);
        Task<TargetLikerDto> GetTargetLikersAsync(int userId, string targetType, int targetId, int page, int pageSize);
    }
}