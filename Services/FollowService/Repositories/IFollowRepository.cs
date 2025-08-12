using System.Threading.Tasks;
using FollowService.Models;

namespace FollowService.Repositories
{
    public interface IFollowRepository
    {
        Task<Follow> CreateFollowAsync(Follow follow);
        Task<Follow> GetFollowAsync(int followerId, int followeeId);
        Task<bool> DeleteFollowAsync(int followerId, int followeeId);
    }
}