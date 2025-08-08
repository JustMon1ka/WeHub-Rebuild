using CircleService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CircleService.Repositories;

/// <summary>
/// 圈子仓储接口，定义了与圈子数据相关的操作。
/// </summary>
public interface ICircleRepository
{
    /// <summary>
    /// 根据ID异步获取一个圈子
    /// </summary>
    /// <param name="id">圈子ID</param>
    /// <returns>返回找到的圈子，如果不存在则返回null</returns>
    Task<Circle?> GetByIdAsync(int id);

    /// <summary>
    /// 异步获取所有圈子的列表，支持按名称模糊搜索
    /// </summary>
    /// <param name="name">可选的圈子名称，用于模糊搜索</param>
    /// <returns>返回所有圈子的列表</returns>
    Task<IEnumerable<Circle>> GetAllAsync(string? name = null);

    /// <summary>
    /// 异步添加一个新圈子
    /// </summary>
    /// <param name="circle">要添加的圈子实体</param>
    Task AddAsync(Circle circle);

    /// <summary>
    /// 异步更新一个已存在的圈子
    /// </summary>
    /// <param name="circle">要更新的圈子实体</param>
    Task UpdateAsync(Circle circle);

    /// <summary>
    /// 异步删除一个圈子
    /// </summary>
    /// <param name="id">要删除的圈子ID</param>
    Task DeleteAsync(int id);
} 