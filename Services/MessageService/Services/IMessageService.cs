using MessageService.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageService.Services
{
    public interface IMessageService
    {
        Task<IEnumerable<ConversationDto>> GetConversationsAsync(int currentUserId);
        Task<IEnumerable<MessageDto>> GetMessagesAsync(int currentUserId, int otherUserId);
        Task<MessageDto> SendMessageAsync(int senderId, int receiverId, string content);
        Task MarkAsReadAsync(int currentUserId, int otherUserId);
    }
}