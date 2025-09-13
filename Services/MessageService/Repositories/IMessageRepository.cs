using MessageService.DTOs;
using MessageService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageService.Repositories
{
    public interface IMessageRepository
    {
        Task<IEnumerable<ConversationDto>> GetConversationsAsync(long? currentUserId);
        Task<IEnumerable<MessageDto>> GetMessagesAsync(long? currentUserId, long? otherUserId);
        Task AddMessageAsync(Message message);
        Task MarkAsReadAsync(long? currentUserId, long? otherUserId);
        Task<int> GetUnreadCountAsync(long? currentUserId);
    }
}