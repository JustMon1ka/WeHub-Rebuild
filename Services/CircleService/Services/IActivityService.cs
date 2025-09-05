using CircleService.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CircleService.Services;

/// <summary>
/// 活动服务接口，定义了与活动相关的业务逻辑。
/// </summary>
public interface IActivityService
{
    /// <summary>
    /// 获取指定圈子的所有活动
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    /// <returns>返回活动信息DTO的列表</returns>
    Task<IEnumerable<ActivityDto>> GetActivitiesByCircleIdAsync(int circleId);

    /// <summary>
    /// 根据活动ID获取活动详情
    /// </summary>
    /// <param name="activityId">活动ID</param>
    /// <returns>返回活动详情，如果不存在则返回null</returns>
    Task<ActivityDto?> GetActivityByIdAsync(int activityId);

    /// <summary>
    /// 在指定圈子中创建一个新活动
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    /// <param name="createActivityDto">创建活动所需的数据</param>
    /// <param name="creatorId">创建者的用户ID</param>
    /// <returns>返回创建成功的活动的DTO</returns>
    Task<ServiceResponse<ActivityDto>> CreateActivityAsync(int circleId, CreateActivityDto createActivityDto, int creatorId);

    /// <summary>
    /// 更新一个活动
    /// </summary>
    /// <param name="activityId">要更新的活动ID</param>
    /// <param name="updateActivityDto">更新活动所需的数据</param>
    /// <param name="modifierId">修改者的用户ID</param>
    /// <returns>操作结果的Service层响应</returns>
    Task<ServiceResponse> UpdateActivityAsync(int activityId, UpdateActivityDto updateActivityDto, int modifierId);

    /// <summary>
    /// 删除一个活动
    /// </summary>
    /// <param name="activityId">要删除的活动ID</param>
    /// <param name="deleterId">删除者的用户ID</param>
    /// <returns>操作结果的Service层响应</returns>
    Task<ServiceResponse> DeleteActivityAsync(int activityId, int deleterId);
}

/// <summary>
/// 泛型版本的Service层响应类，用于在成功时携带数据。
/// </summary>
public class ServiceResponse<T> : ServiceResponse
{
    /// <summary>
    /// 操作成功时返回的数据。
    /// </summary>
    public T? Data { get; private set; }

    /// <summary>
    /// 创建一个表示成功的响应，并携带数据。
    /// </summary>
    public static ServiceResponse<T> Succeed(T data) => new ServiceResponse<T> { Success = true, Data = data };

    /// <summary>
    /// 创建一个表示失败的响应，并指定泛型类型。
    /// </summary>
    /// <param name="errorMessage">错误信息。</param>
    public static new ServiceResponse<T> Fail(string errorMessage) => new ServiceResponse<T> { Success = false, ErrorMessage = errorMessage };
} 