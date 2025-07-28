using CircleService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CircleService.Repositories;

/// <summary>
/// 用户活动参与记录仓储接口，定义了与用户参与活动数据相关的操作。
/// </summary>
public interface IActivityParticipantRepository
{
    /// <summary>
    /// 根据活动ID和用户ID异步获取一个参与记录
    /// </summary>
    /// <param name="activityId">活动ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>返回找到的参与记录，如果不存在则返回null</returns>
    Task<ActivityParticipant?> GetByIdAsync(int activityId, int userId);

    /// <summary>
    /// 根据用户ID异步获取该用户参与的所有活动记录
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>返回该用户的所有活动参与记录列表</returns>
    Task<IEnumerable<ActivityParticipant>> GetByUserIdAsync(int userId);

    /// <summary>
    /// 异步添加一个新的参与记录
    /// </summary>
    /// <param name="participant">要添加的参与记录实体</param>
    Task AddAsync(ActivityParticipant participant);

    /// <summary>
    /// 异步更新一个已存在的参与记录
    /// </summary>
    /// <param name="participant">要更新的参与记录实体</param>
    Task UpdateAsync(ActivityParticipant participant);

    /// <summary>
    /// 异步移除一个用户在特定圈子内的所有活动参与记录
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    /// <param name="userId">用户ID</param>
    Task RemoveUserParticipationInCircleAsync(int circleId, int userId);
} 