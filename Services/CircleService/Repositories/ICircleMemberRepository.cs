using CircleService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CircleService.Repositories;

/// <summary>
/// 圈子成员仓储接口，定义了与圈子成员数据相关的操作。
/// </summary>
public interface ICircleMemberRepository
{
    /// <summary>
    /// 根据圈子ID和用户ID异步获取一个成员关系
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>返回找到的成员关系，如果不存在则返回null</returns>
    Task<CircleMember?> GetByIdAsync(int circleId, int userId);

    /// <summary>
    /// 根据圈子ID异步获取该圈子的所有成员列表
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    /// <returns>返回该圈子的所有成员列表</returns>
    Task<IEnumerable<CircleMember>> GetMembersByCircleIdAsync(int circleId);

    /// <summary>
    /// 异步添加一个新成员
    /// </summary>
    /// <param name="member">要添加的成员实体</param>
    Task AddAsync(CircleMember member);

    /// <summary>
    /// 异步更新一个已存在的成员关系
    /// </summary>
    /// <param name="member">要更新的成员实体</param>
    Task UpdateAsync(CircleMember member);

    /// <summary>
    /// 异步移除一个成员
    /// </summary>
    /// <param name="member">要移除的成员实体</param>
    Task RemoveAsync(CircleMember member);
} 