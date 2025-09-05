using System.Threading.Tasks;

namespace CircleService.Services;

/// <summary>
/// 活动自动完成服务接口
/// </summary>
public interface IActivityAutoCompleteService
{
    /// <summary>
    /// 处理用户发帖事件，自动完成相关活动
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="circleId">圈子ID</param>
    /// <param name="postId">帖子ID</param>
    /// <returns>完成的活动数量</returns>
    Task<int> HandlePostCreatedAsync(int userId, int circleId, int postId);

    /// <summary>
    /// 处理用户点赞事件，自动完成相关活动
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="circleId">圈子ID</param>
    /// <param name="postId">被点赞的帖子ID</param>
    /// <returns>完成的活动数量</returns>
    Task<int> HandlePostLikedAsync(int userId, int circleId, int postId);

    /// <summary>
    /// 处理用户签到事件，自动完成相关活动
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="circleId">圈子ID</param>
    /// <returns>完成的活动数量</returns>
    Task<int> HandleCheckInAsync(int userId, int circleId);
}
