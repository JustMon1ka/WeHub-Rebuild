using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MessageService.DTOs;
using MessageService.Services;
using System;
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

        [HttpGet]
        public async Task<IActionResult> GetConversations()
        {
            try
            {
                if (string.IsNullOrEmpty(User.Identity?.Name))
                    return Unauthorized("用户未认证");

                var conversations = await _messageService.GetConversationsAsync(User.Identity.Name);
                return Ok(conversations);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetConversations Error: {ex}"); // 调试日志
                return StatusCode(500, $"获取会话列表失败: {ex.Message}");
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetMessages(int userId)
        {
            try
            {
                if (string.IsNullOrEmpty(User.Identity?.Name))
                    return Unauthorized("用户未认证");

                var messages = await _messageService.GetMessagesAsync(User.Identity.Name, userId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetMessages Error: {ex}"); // 调试日志
                return StatusCode(500, $"获取聊天记录失败: {ex.Message}");
            }
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> SendMessage(int userId, [FromBody] SendMessageDto messageDto)
        {
            try
            {
                if (string.IsNullOrEmpty(User.Identity?.Name))
                    return Unauthorized("用户未认证");

                if (messageDto == null || string.IsNullOrWhiteSpace(messageDto.Content))
                    return BadRequest("消息内容不能为空");

                var message = await _messageService.SendMessageAsync(User.Identity.Name, userId, messageDto.Content);
                return CreatedAtAction(nameof(GetMessages), new { userId }, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendMessage Error: {ex}"); // 调试日志
                return StatusCode(500, $"发送消息失败: {ex.Message}");
            }
        }

        [HttpPut("{userId}/read")]
        public async Task<IActionResult> MarkAsRead(int userId)
        {
            try
            {
                if (string.IsNullOrEmpty(User.Identity?.Name))
                    return Unauthorized("用户未认证");

                await _messageService.MarkAsReadAsync(User.Identity.Name, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MarkAsRead Error: {ex}"); // 调试日志
                return StatusCode(500, $"标记已读失败: {ex.Message}");
            }
        }
    }
}