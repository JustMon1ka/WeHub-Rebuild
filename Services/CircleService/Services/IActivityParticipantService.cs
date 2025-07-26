using CircleService.DTOs;
using System.Threading.Tasks;

namespace CircleService.Services;

/// <summary>
/// 用户参与活动服务接口，定义了相关的业务逻辑。
/// </summary>
public interface IActivityParticipantService
{
    /// <summary>
    /// 用户报名参加一个活动
    /// </summary>
    /// <param name="activityId">活动ID</param>
    /// <param name="userId">参与用户的ID</param>
    /// <returns>操作结果的Service层响应</returns>
    Task<ServiceResponse> JoinActivityAsync(int activityId, int userId);

    /// <summary>
    /// 用户完成活动任务
    /// </summary>
    /// <param name="activityId">活动ID</param>
    /// <param name="userId">参与用户的ID</param>
    /// <returns>操作结果的Service层响应</returns>
    Task<ServiceResponse> CompleteActivityTaskAsync(int activityId, int userId);

    /// <summary>
    /// 领取活动奖励
    /// </summary>
    /// <param name="activityId">活动ID</param>
    /// <param name="userId">参与用户的ID</param>
    /// <returns>包含奖励信息的Service层响应</returns>
    Task<ServiceResponse<RewardDto>> ClaimRewardAsync(int activityId, int userId);
} 