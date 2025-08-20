using NoticeService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NoticeService.Repositories
{
    public interface INotificationRepository
    {
        Task<Dictionary<string, int>> GetUnreadCountsAsync(int userId);
        Task MarkAsReadAsync(int userId, string type);
        Task<(List<Like> Unread, (List<Like> Items, int Total) Read)> GetLikesAsync(int userId, int page, int pageSize);
        Task<List<Reply>> GetRepliesAsync(int userId, int page, int pageSize, bool unreadOnly);
        Task<List<Repost>> GetRepostsAsync(int userId, int page, int pageSize, bool unreadOnly);
        Task<List<Mention>> GetMentionsAsync(int userId, int page, int pageSize, bool unreadOnly);
        Task<(List<int> Items, int Total)> GetTargetLikersAsync(int userId, string targetType, int targetId, int page, int pageSize);
    }
}