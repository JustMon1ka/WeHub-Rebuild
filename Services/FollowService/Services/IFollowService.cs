using System.Threading.Tasks;
using FollowService.DTOs;

namespace FollowService.Services
{
    public interface IFollowService
    {
        Task<FollowDto> FollowAsync(CreateFollowDto createFollowDto);
        Task<bool> UnfollowAsync(int followeeId);
        Task<UserCountDto> GetFollowCountsAsync(int userId);
        Task<PagedFollowListDto> GetFollowingListAsync(int userId, int page, int pageSize);
        Task<PagedFollowListDto> GetFollowersListAsync(int userId, int page, int pageSize);
    }
}