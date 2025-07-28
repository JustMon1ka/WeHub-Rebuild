using CircleService.DTOs;
using CircleService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CircleService.Services;

/// <summary>
/// 圈子服务接口，定义了与圈子相关的业务逻辑。
/// </summary>
public interface ICircleService
{
    /// <summary>
    /// 根据ID异步获取一个圈子的DTO
    /// </summary>
    /// <param name="id">圈子ID</param>
    /// <returns>返回圈子的数据传输对象，如果不存在则返回null</returns>
    Task<CircleDto?> GetCircleByIdAsync(int id);

    /// <summary>
    /// 异步获取所有圈子的DTO列表，支持按名称模糊搜索
    /// </summary>
    /// <param name="name">可选的圈子名称，用于模糊搜索</param>
    /// <returns>返回所有圈子的DTO列表</returns>
    Task<IEnumerable<CircleDto>> GetAllCirclesAsync(string? name = null);

    /// <summary>
    /// 异步创建一个新圈子
    /// </summary>
    /// <param name="createCircleDto">创建圈子所需的数据</param>
    /// <param name="ownerId">创建者的用户ID</param>
    /// <returns>返回创建成功的圈子的DTO</returns>
    Task<CircleDto> CreateCircleAsync(CreateCircleDto createCircleDto, int ownerId);

    /// <summary>
    /// 异步更新一个圈子
    /// </summary>
    /// <param name="id">要更新的圈子ID</param>
    /// <param name="updateCircleDto">更新圈子所需的数据</param>
    /// <returns>返回一个布尔值，表示更新是否成功</returns>
    Task<bool> UpdateCircleAsync(int id, UpdateCircleDto updateCircleDto);

    /// <summary>
    /// 异步删除一个圈子
    /// </summary>
    /// <param name="id">要删除的圈子ID</param>
    /// <param name="deleterId">执行删除操作的用户ID</param>
    /// <returns>返回一个布尔值，表示删除是否成功</returns>
    Task<bool> DeleteCircleAsync(int id, int deleterId);
} 