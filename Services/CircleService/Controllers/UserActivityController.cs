using CircleService.DTOs;
using CircleService.Services;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CircleService.Controllers;

/// <summary>
/// 用户活动参与状态查询API控制器
/// </summary>
[ApiController]
[Route("api/activity-participants")]
[Authorize] // 默认需要认证
public class UserActivityController : ControllerBase
{
    private readonly IActivityParticipantService _participantService;

    public UserActivityController(IActivityParticipantService participantService)
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
    /// 获取用户参与的所有活动状态
    /// </summary>
    /// <param name="userId">用户ID</param>
    [HttpGet("user/{userId}/participations")]
    public async Task<IActionResult> GetUserActivityParticipations(int userId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized(BaseHttpResponse.Fail(401, "用户身份验证失败"));
            }
            
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
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized(BaseHttpResponse.Fail(401, "用户身份验证失败"));
            }
            
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
