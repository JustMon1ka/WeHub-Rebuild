using System.Collections.Generic;
using System.Threading.Tasks;
using FollowService.Models;

namespace FollowService.Repositories
{
    public interface IFollowRepository
    {
        Task<Follow> CreateFollowAsync(Follow follow);
        Task<Follow?> GetFollowAsync(int followerId, int followeeId);
        Task<bool> DeleteFollowAsync(int followerId, int followeeId);
        Task<int> GetFollowingCountAsync(int userId);
        Task<int> GetFollowerCountAsync(int userId);
        Task<List<Follow>> GetFollowingListAsync(int userId, int page, int pageSize);
        Task<List<Follow>> GetFollowerListAsync(int userId, int page, int pageSize);
    }
}