using MessageService.DTOs;
using MessageService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageService.Repositories
{
    public interface IMessageRepository
    {
        Task<IEnumerable<ConversationDto>> GetConversationsAsync(int currentUserId);
        Task<IEnumerable<MessageDto>> GetMessagesAsync(int currentUserId, int otherUserId);
        Task AddMessageAsync(Message message);
        Task MarkAsReadAsync(int currentUserId, int otherUserId);
    }
}