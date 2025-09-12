using MessageService.Data;
using MessageService.DTOs;
using MessageService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageService.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly AppDbContext _context;

        public MessageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ConversationDto>> GetConversationsAsync(long? currentUserId)
        {
            try
            {
                Console.WriteLine($"[MessageRepository] GetConversationsAsync called with currentUserId={currentUserId}");

                // 先获取所有消息，然后在内存中处理
                var allMessages = await _context.Messages
                    .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                    .ToListAsync();

                Console.WriteLine($"[MessageRepository] Found {allMessages.Count} total messages");

                var conversations = allMessages
                    .GroupBy(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
                    .Select(g => new
                    {
                        OtherUserId = g.Key,
                        LastMessage = g.OrderByDescending(m => m.SentAt).FirstOrDefault(),
                        UnreadCount = g.Count(m => m.ReceiverId == currentUserId && !m.IsRead)
                    })
                    .ToList();

                Console.WriteLine($"[MessageRepository] Found {conversations.Count} conversations");

                var result = conversations.Select(c => new ConversationDto
                {
                    OtherUserId = c.OtherUserId,
                    LastMessage = c.LastMessage != null ? new MessageDto
                    {
                        MessageId = c.LastMessage.MessageId,
                        SenderId = c.LastMessage.SenderId,
                        ReceiverId = c.LastMessage.ReceiverId,
                        Content = c.LastMessage.Content,
                        SentAt = c.LastMessage.SentAt,
                        IsRead = c.LastMessage.IsRead
                    } : null,
                    UnreadCount = c.UnreadCount
                }).ToList();

                Console.WriteLine($"[MessageRepository] Returning {result.Count} conversation DTOs");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MessageRepository] Error in GetConversationsAsync: {ex}");
                throw;
            }
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(long? currentUserId, long? otherUserId)
        {
            var messages = await _context.Messages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == otherUserId) ||
                            (m.SenderId == otherUserId && m.ReceiverId == currentUserId))
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    MessageId = m.MessageId,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Content = m.Content,
                    SentAt = m.SentAt,
                    IsRead = m.IsRead
                })
                .ToListAsync();

            return messages.Select(m => new MessageDto
            {
                MessageId = m.MessageId,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                Content = m.Content,
                SentAt = m.SentAt,
                IsRead = m.IsRead
            });
        }

        public async Task AddMessageAsync(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(long? currentUserId, long? otherUserId)
        {
            try
            {
                Console.WriteLine($"[MessageRepository] MarkAsReadAsync called with currentUserId={currentUserId}, otherUserId={otherUserId}");

                var messages = await _context.Messages
                    .Where(m => m.ReceiverId == currentUserId && m.SenderId == otherUserId)
                    .ToListAsync();

                Console.WriteLine($"[MessageRepository] Found {messages.Count} messages to mark as read");

                var unreadMessages = messages.Where(m => !m.IsRead).ToList();
                Console.WriteLine($"[MessageRepository] Found {unreadMessages.Count} unread messages");

                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                    Console.WriteLine($"[MessageRepository] Marking message {message.MessageId} as read");
                }

                if (unreadMessages.Count > 0)
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"[MessageRepository] Successfully saved changes");
                }
                else
                {
                    Console.WriteLine($"[MessageRepository] No unread messages to update");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MessageRepository] Error in MarkAsReadAsync: {ex}");
                throw;
            }
        }
    }
}