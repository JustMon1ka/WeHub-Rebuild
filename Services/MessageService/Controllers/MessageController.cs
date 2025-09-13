using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MessageService.DTOs;
using MessageService.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MessageService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/Messages")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessagesController(IMessageService messageService)
        {
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        }

        // 获取当前用户ID
        private long? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("无法获取当前用户ID");
            }
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetConversations()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                Console.WriteLine($"[MessageController] GetConversations called with userId: {currentUserId}");
                var conversations = await _messageService.GetConversationsAsync(currentUserId);
                Console.WriteLine($"[MessageController] Found {conversations.Count()} conversations");

                // 返回标准格式
                var response = new
                {
                    code = 200,
                    msg = "OK",
                    data = conversations
                };
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"[MessageController] Unauthorized: {ex.Message}");
                return Unauthorized(new { code = 401, msg = ex.Message, data = (object)null });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetConversations Error: {ex}");
                return StatusCode(500, new { code = 500, msg = $"获取会话列表失败: {ex.Message}", data = (object)null });
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetMessages(long userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var messages = await _messageService.GetMessagesAsync(currentUserId, userId);

                // 返回标准格式
                var response = new
                {
                    code = 200,
                    msg = "OK",
                    data = messages
                };
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { code = 401, msg = ex.Message, data = (object)null });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetMessages Error: {ex}");
                return StatusCode(500, new { code = 500, msg = $"获取聊天记录失败: {ex.Message}", data = (object)null });
            }
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> SendMessage(long userId, [FromBody] SendMessageDto messageDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                if (messageDto == null || string.IsNullOrWhiteSpace(messageDto.Content))
                    return BadRequest(new { code = 400, msg = "消息内容不能为空", data = (object)null });

                var message = await _messageService.SendMessageAsync(currentUserId, userId, messageDto.Content);

                // 返回标准格式
                var response = new
                {
                    code = 200,
                    msg = "OK",
                    data = new
                    {
                        messageId = message.MessageId,
                        success = true
                    }
                };
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { code = 401, msg = ex.Message, data = (object)null });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendMessage Error: {ex}");
                return StatusCode(500, new { code = 500, msg = $"发送消息失败: {ex.Message}", data = (object)null });
            }
        }

        [HttpPut("{userId}/read")]
        public async Task<IActionResult> MarkAsRead(long userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                await _messageService.MarkAsReadAsync(currentUserId, userId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MarkAsRead Error: {ex}");
                return StatusCode(500, $"标记已读失败: {ex.Message}");
            }
        }

        // 临时测试端点，不需要认证
        [HttpGet("test")]
        public async Task<IActionResult> TestConversations()
        {
            try
            {
                Console.WriteLine("[MessageController] TestConversations called");
                // 使用固定的用户ID进行测试
                var testUserId = 100140L;
                var conversations = await _messageService.GetConversationsAsync(testUserId);
                Console.WriteLine($"[MessageController] Test found {conversations.Count()} conversations");

                // 输出每个会话的详细信息
                foreach (var conv in conversations)
                {
                    Console.WriteLine($"[MessageController] Conversation: OtherUserId={conv.OtherUserId}, UnreadCount={conv.UnreadCount}");
                    if (conv.LastMessage != null)
                    {
                        Console.WriteLine($"[MessageController] LastMessage: MessageId={conv.LastMessage.MessageId}, Content={conv.LastMessage.Content}");
                    }
                }

                return Ok(conversations);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestConversations Error: {ex}");
                return StatusCode(500, $"测试获取会话列表失败: {ex.Message}");
            }
        }
    }
}