using System.Threading.Tasks;
using FollowService.DTOs;

namespace FollowService.Services
{
    public interface IFollowService
    {
        Task<FollowDto> FollowAsync(CreateFollowDto createFollowDto);
        Task<bool> UnfollowAsync(int followeeId);
        Task<UserCountDto> GetFollowCountsAsync();
        Task<PagedFollowListDto> GetFollowingListAsync(int page, int pageSize);
        Task<PagedFollowListDto> GetFollowersListAsync(int page, int pageSize);
    }
}