using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoticeService.DTOs;
using NoticeService.Services;
using StackExchange.Redis;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NoticeService.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConnectionMultiplexer _redis;

        public NotificationsController(INotificationService notificationService, IHttpContextAccessor httpContextAccessor, IConnectionMultiplexer redis)
        {
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
            _redis = redis;
        }

        [HttpGet]
        public async Task<ActionResult<NotificationSummaryDto>> GetNotificationSummary()
        {
            var userId = GetCurrentUserId();
            var redisDb = _redis.GetDatabase();
            var summary = await _notificationService.GetNotificationSummaryAsync(userId, redisDb);
            return Ok(summary);
        }

        [HttpPost("read")]
        public async Task<ActionResult> MarkAsRead([FromBody] MarkReadDto dto)
        {
            var userId = GetCurrentUserId();
            var redisDb = _redis.GetDatabase();
            await _notificationService.MarkAsReadAsync(userId, dto.Type, redisDb);
            return Ok(new { success = true });
        }

        [HttpGet("likes")]
        public async Task<ActionResult<LikeResponseDto>> GetLikes(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            var redisDb = _redis.GetDatabase();
            var likes = await _notificationService.GetLikesAsync(userId, page, pageSize, redisDb);
            return Ok(likes);
        }

        [HttpGet("replies")]
        public async Task<ActionResult<List<ReplyNotificationDto>>> GetReplies(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool unreadOnly = false)
        {
            var userId = GetCurrentUserId();
            var redisDb = _redis.GetDatabase();
            var replies = await _notificationService.GetRepliesAsync(userId, page, pageSize, unreadOnly, redisDb);
            return Ok(new { total = replies.Count, items = replies });
        }

        [HttpGet("reposts")]
        public async Task<ActionResult<List<RepostNotificationDto>>> GetReposts(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool unreadOnly = false)
        {
            var userId = GetCurrentUserId();
            var redisDb = _redis.GetDatabase();
            var reposts = await _notificationService.GetRepostsAsync(userId, page, pageSize, unreadOnly, redisDb);
            return Ok(new { total = reposts.Count, items = reposts });
        }

        [HttpGet("mentions")]
        public async Task<ActionResult<List<MentionNotificationDto>>> GetMentions(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool unreadOnly = false)
        {
            var userId = GetCurrentUserId();
            var redisDb = _redis.GetDatabase();
            var mentions = await _notificationService.GetMentionsAsync(userId, page, pageSize, unreadOnly, redisDb);
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