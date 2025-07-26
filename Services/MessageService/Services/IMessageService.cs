using MessageService.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageService.Services
{
    public interface IMessageService
    {
        Task<IEnumerable<ConversationDto>> GetConversationsAsync(string currentUserId);
        Task<IEnumerable<MessageDto>> GetMessagesAsync(string currentUserId, int otherUserId);
        Task<MessageDto> SendMessageAsync(string senderId, int receiverId, string content);
        Task MarkAsReadAsync(string currentUserId, int otherUserId);
    }
}