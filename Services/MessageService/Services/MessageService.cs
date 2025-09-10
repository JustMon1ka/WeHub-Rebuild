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

        public async Task<IEnumerable<ConversationDto>> GetConversationsAsync(string currentUserId)
        {
            if (string.IsNullOrEmpty(currentUserId))
                throw new ArgumentException("用户ID不能为空");

            return await _messageRepository.GetConversationsAsync(int.Parse(currentUserId));
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(string currentUserId, int otherUserId)
        {
            if (string.IsNullOrEmpty(currentUserId))
                throw new ArgumentException("用户ID不能为空");

            return await _messageRepository.GetMessagesAsync(int.Parse(currentUserId), otherUserId);
        }

        public async Task<MessageDto> SendMessageAsync(string senderId, int receiverId, string content)
        {
            if (string.IsNullOrEmpty(senderId))
                throw new ArgumentException("发送者ID不能为空");
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("消息内容不能为空");

            var message = new Message
            {
                SenderId = int.Parse(senderId),
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

        public async Task MarkAsReadAsync(string currentUserId, int otherUserId)
        {
            if (string.IsNullOrEmpty(currentUserId))
                throw new ArgumentException("用户ID不能为空");

            await _messageRepository.MarkAsReadAsync(int.Parse(currentUserId), otherUserId);
        }
    }
}