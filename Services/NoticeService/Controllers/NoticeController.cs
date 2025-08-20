using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoticeService.DTOs;
using NoticeService.Services;
using System.Security.Claims;

namespace NoticeService.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationsController(INotificationService notificationService, IHttpContextAccessor httpContextAccessor)
        {
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<ActionResult<NotificationSummaryDto>> GetNotificationSummary()
        {
            var userId = GetCurrentUserId();
            var summary = await _notificationService.GetNotificationSummaryAsync(userId);
            return Ok(summary);
        }

        [HttpPost("read")]
        public async Task<ActionResult> MarkAsRead([FromBody] MarkReadDto dto)
        {
            var userId = GetCurrentUserId();
            await _notificationService.MarkAsReadAsync(userId, dto.Type);
            return Ok(new { success = true });
        }

        [HttpGet("likes")]
        public async Task<ActionResult<LikeResponseDto>> GetLikes(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            var likes = await _notificationService.GetLikesAsync(userId, page, pageSize);
            return Ok(likes);
        }

        [HttpGet("replies")]
        public async Task<ActionResult<object>> GetReplies(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool unreadOnly = false)
        {
            var userId = GetCurrentUserId();
            var replies = await _notificationService.GetRepliesAsync(userId, page, pageSize, unreadOnly);
            return Ok(new { total = replies.Count, items = replies });
        }

        [HttpGet("reposts")]
        public async Task<ActionResult<object>> GetReposts(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool unreadOnly = false)
        {
            var userId = GetCurrentUserId();
            var reposts = await _notificationService.GetRepostsAsync(userId, page, pageSize, unreadOnly);
            return Ok(new { total = reposts.Count, items = reposts });
        }

        [HttpGet("mentions")]
        public async Task<ActionResult<object>> GetMentions(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool unreadOnly = false)
        {
            var userId = GetCurrentUserId();
            var mentions = await _notificationService.GetMentionsAsync(userId, page, pageSize, unreadOnly);
            return Ok(new { total = mentions.Count, items = mentions });
        }

        [HttpGet("likes/target")]
        public async Task<ActionResult<TargetLikerDto>> GetTargetLikers(
            [FromQuery] string targetType, [FromQuery] int targetId,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (string.IsNullOrEmpty(targetType) || !new[] { "post", "comment" }.Contains(targetType.ToLower()))
                return BadRequest("无效的 targetType");

            var userId = GetCurrentUserId();
            var likers = await _notificationService.GetTargetLikersAsync(userId, targetType, targetId, page, pageSize);
            return Ok(likers);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("无法获取当前用户ID");
            }
            return userId;
        }
    }
}