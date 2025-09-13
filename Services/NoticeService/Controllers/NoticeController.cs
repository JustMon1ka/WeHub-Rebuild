using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoticeService.Data;
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
        private readonly NoticeDbContext _context;

        public NotificationsController(INotificationService notificationService, IHttpContextAccessor httpContextAccessor, IConnectionMultiplexer redis, NoticeDbContext context)
        {
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
            _redis = redis;
            _context = context;
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

        [HttpGet("comments")]
        public async Task<ActionResult<object>> GetComments(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool unreadOnly = false)
        {
            var userId = GetCurrentUserId();
            var redisDb = _redis.GetDatabase();
            var comments = await _notificationService.GetCommentsAsync(userId, page, pageSize, unreadOnly, redisDb);
            return Ok(new { total = comments.Count, items = comments });
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

        [HttpPost("likes")]
        public async Task<ActionResult> CreateLikeNotification([FromBody] CreateLikeNotificationDto dto)
        {
            try
            {
                var redisDb = _redis.GetDatabase();
                await _notificationService.CreateLikeNotificationAsync(dto, redisDb);
                return Ok(new { success = true, message = "点赞通知创建成功" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"创建点赞通知失败: {ex.Message}" });
            }
        }

        // 简单的健康检查端点
        [HttpGet("health")]
        [AllowAnonymous]
        public ActionResult HealthCheck()
        {
            Console.WriteLine("[NoticeController] Health check endpoint called");
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }

        // 测试数据库连接
        [HttpGet("test-db")]
        [AllowAnonymous]
        public async Task<ActionResult> TestDatabase()
        {
            try
            {
                Console.WriteLine("[NoticeController] TestDatabase called");
                var commentCount = await _context.Comments.CountAsync();
                Console.WriteLine($"[NoticeController] Total comments in database: {commentCount}");
                return Ok(new { totalComments = commentCount });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NoticeController] TestDatabase error: {ex}");
                return BadRequest(new { error = ex.Message });
            }
        }

        // 测试Redis数据
        [HttpGet("test-redis")]
        [AllowAnonymous]
        public async Task<ActionResult> TestRedis([FromQuery] int userId = 100260)
        {
            try
            {
                Console.WriteLine($"[NoticeController] TestRedis called for userId: {userId}");
                var redisDb = _redis.GetDatabase();

                // 检查各种类型的未读通知
                var types = new[] { "comment", "reply", "like", "repost", "mention" };
                var results = new Dictionary<string, object>();

                foreach (var type in types)
                {
                    var key = $"unread-notice:{userId}:{type}";
                    var count = await redisDb.ListLengthAsync(key);
                    var items = await redisDb.ListRangeAsync(key, 0, -1);

                    results[type] = new { count, items = items.Select(x => x.ToString()).ToArray() };
                    Console.WriteLine($"[NoticeController] Redis {type}: count={count}, items=[{string.Join(", ", items.Select(x => x.ToString()))}]");
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NoticeController] TestRedis error: {ex}");
                return BadRequest(new { error = ex.Message });
            }
        }

        // 测试评论通知端点，不需要认证
        [HttpGet("test-comments")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> TestGetComments(
            [FromQuery] int userId = 100260,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool unreadOnly = false)
        {
            try
            {
                Console.WriteLine($"[NoticeController] TestGetComments called: userId={userId}, page={page}, pageSize={pageSize}, unreadOnly={unreadOnly}");

                // 测试Redis连接
                var redisDb = _redis.GetDatabase();
                Console.WriteLine($"[NoticeController] Redis connection: {redisDb != null}");

                // 测试数据库连接
                Console.WriteLine($"[NoticeController] Testing database connection...");

                // 直接调用服务
                var comments = await _notificationService.GetCommentsAsync(userId, page, pageSize, unreadOnly, redisDb);
                Console.WriteLine($"[NoticeController] GetCommentsAsync completed: {comments.Count} comments");

                var result = new { total = comments.Count, items = comments };
                Console.WriteLine($"[NoticeController] TestGetComments result: {comments.Count} comments");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NoticeController] TestGetComments error: {ex}");
                Console.WriteLine($"[NoticeController] Stack trace: {ex.StackTrace}");
                return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // 临时测试端点，不需要认证
        [HttpPost("test-likes")]
        [AllowAnonymous]
        public async Task<ActionResult> TestCreateLikeNotification([FromBody] CreateLikeNotificationDto dto)
        {
            try
            {
                Console.WriteLine($"[NoticeController] TestCreateLikeNotification called with: {System.Text.Json.JsonSerializer.Serialize(dto)}");

                // 测试Redis连接
                var redisDb = _redis.GetDatabase();
                Console.WriteLine($"[NoticeController] Redis connection test: {redisDb != null}");

                // 测试数据库连接
                Console.WriteLine($"[NoticeController] Testing database connection...");

                // 直接调用服务
                await _notificationService.CreateLikeNotificationAsync(dto, redisDb);
                Console.WriteLine($"[NoticeController] NotificationService.CreateLikeNotificationAsync completed successfully");

                return Ok(new { success = true, message = "测试点赞通知创建成功" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NoticeController] TestCreateLikeNotification error: {ex}");
                Console.WriteLine($"[NoticeController] Stack trace: {ex.StackTrace}");
                return BadRequest(new { success = false, message = $"测试创建点赞通知失败: {ex.Message}" });
            }
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