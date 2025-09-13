using NoticeService.Models;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NoticeService.Repositories
{
    public interface INotificationRepository
    {
        Task<Dictionary<string, int>> GetUnreadCountsAsync(int userId, IDatabase redis);
        Task MarkAsReadAsync(int userId, string type, IDatabase redis);
        Task<(List<Like> Unread, (List<Like> Items, int Total) Read)> GetLikesAsync(int userId, int page, int pageSize, IDatabase redis);
        Task<List<Reply>> GetRepliesAsync(int userId, int page, int pageSize, bool unreadOnly, IDatabase redis);
        Task<List<Repost>> GetRepostsAsync(int userId, int page, int pageSize, bool unreadOnly, IDatabase redis);
        Task<List<Mention>> GetMentionsAsync(int userId, int page, int pageSize, bool unreadOnly, IDatabase redis);
        Task<(List<int> Items, int Total)> GetTargetLikersAsync(int userId, string targetType, int targetId, int page, int pageSize);
        Task<List<Comment>> GetCommentsAsync(int userId, int page, int pageSize, bool unreadOnly, IDatabase redis);
        Task CreateLikeAsync(Like like);
    }
}