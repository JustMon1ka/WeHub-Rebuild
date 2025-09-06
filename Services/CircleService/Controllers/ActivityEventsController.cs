using CircleService.Services;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CircleService.Controllers;

/// <summary>
/// 活动事件处理控制器
/// </summary>
[ApiController]
[Route("api/activity-events")]
public class ActivityEventsController : ControllerBase
{
    private readonly IActivityAutoCompleteService _autoCompleteService;

    public ActivityEventsController(IActivityAutoCompleteService autoCompleteService)
    {
        _autoCompleteService = autoCompleteService;
    }

    /// <summary>
    /// 处理用户发帖事件
    /// </summary>
    /// <param name="request">发帖事件请求</param>
    [HttpPost("post-created")]
    public async Task<IActionResult> HandlePostCreated([FromBody] PostCreatedEventRequest request)
    {
        try
        {
            var completedCount = await _autoCompleteService.HandlePostCreatedAsync(
                request.UserId, 
                request.CircleId, 
                request.PostId);

            return Ok(BaseHttpResponse<object>.Success(new { CompletedActivities = completedCount }, 
                $"发帖事件处理完成，自动完成了 {completedCount} 个活动"));
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, BaseHttpResponse.Fail(500, $"处理发帖事件失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 处理用户点赞事件
    /// </summary>
    /// <param name="request">点赞事件请求</param>
    [HttpPost("post-liked")]
    public async Task<IActionResult> HandlePostLiked([FromBody] PostLikedEventRequest request)
    {
        try
        {
            var completedCount = await _autoCompleteService.HandlePostLikedAsync(
                request.UserId, 
                request.CircleId, 
                request.PostId);

            return Ok(BaseHttpResponse<object>.Success(new { CompletedActivities = completedCount }, 
                $"点赞事件处理完成，自动完成了 {completedCount} 个活动"));
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, BaseHttpResponse.Fail(500, $"处理点赞事件失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 处理用户签到事件
    /// </summary>
    /// <param name="request">签到事件请求</param>
    [HttpPost("check-in")]
    public async Task<IActionResult> HandleCheckIn([FromBody] CheckInEventRequest request)
    {
        try
        {
            var completedCount = await _autoCompleteService.HandleCheckInAsync(
                request.UserId, 
                request.CircleId);

            return Ok(BaseHttpResponse<object>.Success(new { CompletedActivities = completedCount }, 
                $"签到事件处理完成，自动完成了 {completedCount} 个活动"));
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, BaseHttpResponse.Fail(500, $"处理签到事件失败: {ex.Message}"));
        }
    }
}

/// <summary>
/// 发帖事件请求
/// </summary>
public class PostCreatedEventRequest
{
    public int UserId { get; set; }
    public int CircleId { get; set; }
    public int PostId { get; set; }
}

/// <summary>
/// 点赞事件请求
/// </summary>
public class PostLikedEventRequest
{
    public int UserId { get; set; }
    public int CircleId { get; set; }
    public int PostId { get; set; }
}

/// <summary>
/// 签到事件请求
/// </summary>
public class CheckInEventRequest
{
    public int UserId { get; set; }
    public int CircleId { get; set; }
}
