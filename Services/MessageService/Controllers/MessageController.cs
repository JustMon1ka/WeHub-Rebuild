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
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessagesController(IMessageService messageService)
        {
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        }

        // 获取当前用户ID
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
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
                var conversations = await _messageService.GetConversationsAsync(currentUserId);
                return Ok(conversations);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetConversations Error: {ex}");
                return StatusCode(500, $"获取会话列表失败: {ex.Message}");
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetMessages(int userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var messages = await _messageService.GetMessagesAsync(currentUserId, userId);
                return Ok(messages);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetMessages Error: {ex}");
                return StatusCode(500, $"获取聊天记录失败: {ex.Message}");
            }
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> SendMessage(int userId, [FromBody] SendMessageDto messageDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                if (messageDto == null || string.IsNullOrWhiteSpace(messageDto.Content))
                    return BadRequest("消息内容不能为空");

                var message = await _messageService.SendMessageAsync(currentUserId, userId, messageDto.Content);
                return CreatedAtAction(nameof(GetMessages), new { userId }, message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendMessage Error: {ex}");
                return StatusCode(500, $"发送消息失败: {ex.Message}");
            }
        }

        [HttpPut("{userId}/read")]
        public async Task<IActionResult> MarkAsRead(int userId)
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
    }
}