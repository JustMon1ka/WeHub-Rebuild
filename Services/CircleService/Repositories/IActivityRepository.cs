using CircleService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CircleService.Repositories;

/// <summary>
/// 活动仓储接口，定义了与活动数据相关的操作。
/// </summary>
public interface IActivityRepository
{
    /// <summary>
    /// 根据ID异步获取一个活动
    /// </summary>
    /// <param name="id">活动ID</param>
    /// <returns>返回找到的活动，如果不存在则返回null</returns>
    Task<Activity?> GetByIdAsync(int id);

    /// <summary>
    /// 根据圈子ID异步获取该圈子的所有活动列表
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    /// <returns>返回该圈子的所有活动列表</returns>
    Task<IEnumerable<Activity>> GetByCircleIdAsync(int circleId);

    /// <summary>
    /// 异步添加一个新活动
    /// </summary>
    /// <param name="activity">要添加的活动实体</param>
    Task AddAsync(Activity activity);

    /// <summary>
    /// 异步更新一个已存在的活动
    /// </summary>
    /// <param name="activity">要更新的活动实体</param>
    Task UpdateAsync(Activity activity);

    /// <summary>
    /// 异步删除一个活动
    /// </summary>
    /// <param name="id">要删除的活动ID</param>
    Task DeleteAsync(int id);
} 