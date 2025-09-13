using MessageService.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageService.Services
{
    public interface IMessageService
    {
        Task<IEnumerable<ConversationDto>> GetConversationsAsync(long? currentUserId);
        Task<IEnumerable<MessageDto>> GetMessagesAsync(long? currentUserId, long? otherUserId);
        Task<MessageDto> SendMessageAsync(long? senderId, long? receiverId, string content);
        Task MarkAsReadAsync(long? currentUserId, long? otherUserId);
        Task<int> GetUnreadCountAsync(long? currentUserId);
    }
}