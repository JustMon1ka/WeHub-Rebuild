using MessageService.DTOs;
using MessageService.Models;
using MessageService.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageService.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;

        public MessageService(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public async Task<IEnumerable<ConversationDto>> GetConversationsAsync(long? currentUserId)
        {
            if (currentUserId <= 0)
                throw new ArgumentException("用户ID不能为空或无效");

            return await _messageRepository.GetConversationsAsync(currentUserId);
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(long? currentUserId, long? otherUserId)
        {
            if (currentUserId <= 0)
                throw new ArgumentException("当前用户ID不能为空或无效");
            if (otherUserId <= 0)
                throw new ArgumentException("对方用户ID不能为空或无效");

            return await _messageRepository.GetMessagesAsync(currentUserId, otherUserId);
        }

        public async Task<MessageDto> SendMessageAsync(long? senderId, long? receiverId, string content)
        {
            if (senderId <= 0)
                throw new ArgumentException("发送者ID不能为空或无效");
            if (receiverId <= 0)
                throw new ArgumentException("接收者ID不能为空或无效");
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("消息内容不能为空");

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            await _messageRepository.AddMessageAsync(message);
            return new MessageDto
            {
                MessageId = message.MessageId,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Content = message.Content,
                SentAt = message.SentAt,
                IsRead = message.IsRead
            };
        }

        public async Task MarkAsReadAsync(long? currentUserId, long? otherUserId)
        {
            if (currentUserId <= 0)
                throw new ArgumentException("当前用户ID不能为空或无效");
            if (otherUserId <= 0)
                throw new ArgumentException("对方用户ID不能为空或无效");

            await _messageRepository.MarkAsReadAsync(currentUserId, otherUserId);
        }
    }
}