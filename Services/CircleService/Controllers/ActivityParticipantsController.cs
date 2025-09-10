using CircleService.DTOs;
using CircleService.Services;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CircleService.Controllers;

/// <summary>
/// 用户参与活动的API控制器
/// </summary>
[ApiController]
[Route("api/activities/{activityId}/participants")]
[Authorize] // 默认需要认证
public class ActivityParticipantsController : ControllerBase
{
    private readonly IActivityParticipantService _participantService;

    public ActivityParticipantsController(IActivityParticipantService participantService)
    {
        _participantService = participantService;
    }

    /// <summary>
    /// 从JWT Claims中获取当前用户ID
    /// </summary>
    /// <returns>用户ID，如果获取失败则返回null</returns>
    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        return null;
    }

    /// <summary>
    /// 报名参加一个活动
    /// </summary>
    /// <param name="activityId">活动ID</param>
    [HttpPost("join")]
    public async Task<IActionResult> JoinActivity(int activityId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(BaseHttpResponse.Fail(401, "用户身份验证失败"));
        }

        var response = await _participantService.JoinActivityAsync(activityId, userId.Value);
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
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(BaseHttpResponse.Fail(401, "用户身份验证失败"));
        }

        var response = await _participantService.CompleteActivityTaskAsync(activityId, userId.Value);
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
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(BaseHttpResponse.Fail(401, "用户身份验证失败"));
        }
        
        var response = await _participantService.ClaimRewardAsync(activityId, userId.Value);
        if (!response.Success)
        {
            return BadRequest(BaseHttpResponse.Fail(400, response.ErrorMessage!));
        }
        return Ok(BaseHttpResponse<object>.Success(response.Data, "奖励领取成功。"));
    }

} 