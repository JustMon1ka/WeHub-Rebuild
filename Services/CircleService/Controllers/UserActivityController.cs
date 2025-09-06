using CircleService.DTOs;
using CircleService.Services;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CircleService.Controllers;

/// <summary>
/// 用户活动参与状态查询API控制器
/// </summary>
[ApiController]
[Route("api/activity-participants")]
public class UserActivityController : ControllerBase
{
    private readonly IActivityParticipantService _participantService;

    public UserActivityController(IActivityParticipantService participantService)
    {
        _participantService = participantService;
    }

    /// <summary>
    /// 获取用户参与的所有活动状态
    /// </summary>
    /// <param name="userId">用户ID</param>
    [HttpGet("user/{userId}/participations")]
    public async Task<IActionResult> GetUserActivityParticipations(int userId)
    {
        try
        {
            // TODO: 实际应从用户认证信息中获取当前用户ID，并验证权限
            var currentUserId = 2; // 临时硬编码，后续需要从JWT Token获取
            
            // 权限验证：用户只能查看自己的参与状态
            if (currentUserId != userId)
            {
                return Forbid("无权查看其他用户的活动参与状态");
            }

            var participations = await _participantService.GetUserActivityParticipationsAsync(userId);
            return Ok(BaseHttpResponse<object>.Success(participations, "获取用户活动参与状态成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, BaseHttpResponse.Fail(500, $"获取用户活动参与状态失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 检查用户是否参与了指定活动
    /// </summary>
    /// <param name="activityId">活动ID</param>
    /// <param name="userId">用户ID</param>
    [HttpGet("activity/{activityId}/user/{userId}")]
    public async Task<IActionResult> GetUserActivityParticipation(int activityId, int userId)
    {
        try
        {
            // TODO: 实际应从用户认证信息中获取当前用户ID，并验证权限
            var currentUserId = 2; // 临时硬编码，后续需要从JWT Token获取
            
            // 权限验证：用户只能查看自己的参与状态
            if (currentUserId != userId)
            {
                return Forbid("无权查看其他用户的活动参与状态");
            }

            var participation = await _participantService.GetUserActivityParticipationAsync(activityId, userId);
            if (participation == null)
            {
                return Ok(BaseHttpResponse<object>.Success(null, "用户未参与该活动"));
            }
            
            return Ok(BaseHttpResponse<object>.Success(participation, "获取用户活动参与状态成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, BaseHttpResponse.Fail(500, $"获取用户活动参与状态失败: {ex.Message}"));
        }
    }
}
