using CircleService.DTOs;
using CircleService.Services;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CircleService.Controllers;

/// <summary>
/// 用户参与活动的API控制器
/// </summary>
[ApiController]
[Route("api/activities/{activityId}/participants")]
public class ActivityParticipantsController : ControllerBase
{
    private readonly IActivityParticipantService _participantService;

    public ActivityParticipantsController(IActivityParticipantService participantService)
    {
        _participantService = participantService;
    }

    /// <summary>
    /// 报名参加一个活动
    /// </summary>
    /// <param name="activityId">活动ID</param>
    [HttpPost("join")]
    public async Task<IActionResult> JoinActivity(int activityId)
    {
        // 假设从JWT Token中获取当前用户ID，这里暂时用一个硬编码代替
        var userId = 2; // TODO: 实际应从用户认证信息中获取

        var response = await _participantService.JoinActivityAsync(activityId, userId);
        if (!response.Success)
        {
            return BadRequest(BaseHttpResponse.Fail(400, response.ErrorMessage!));
        }
        return Ok(BaseHttpResponse<object>.Success(null, "成功参加活动。"));
    }

    /// <summary>
    /// 完成活动任务
    /// </summary>
    /// <param name="activityId">活动ID</param>
    [HttpPut("complete")]
    public async Task<IActionResult> CompleteActivityTask(int activityId)
    {
        // 假设从JWT Token中获取当前用户ID，这里暂时用一个硬编码代替
        var userId = 2; // TODO: 实际应从用户认证信息中获取

        var response = await _participantService.CompleteActivityTaskAsync(activityId, userId);
        if (!response.Success)
        {
            return BadRequest(BaseHttpResponse.Fail(400, response.ErrorMessage!));
        }
        return Ok(BaseHttpResponse<object>.Success(null, "活动任务已完成。"));
    }

    /// <summary>
    /// 领取活动奖励
    /// </summary>
    /// <param name="activityId">活动ID</param>
    [HttpPost("claim-reward")]
    public async Task<IActionResult> ClaimReward(int activityId)
    {
        // 假设从JWT Token中获取当前用户ID，这里暂时用一个硬编码代替
        var userId = 2; // TODO: 实际应从用户认证信息中获取
        
        var response = await _participantService.ClaimRewardAsync(activityId, userId);
        if (!response.Success)
        {
            return BadRequest(BaseHttpResponse.Fail(400, response.ErrorMessage!));
        }
        return Ok(BaseHttpResponse<object>.Success(response.Data, "奖励领取成功。"));
    }

} 