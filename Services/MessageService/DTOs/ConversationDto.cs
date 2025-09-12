namespace MessageService.DTOs
{
    public class ConversationDto
    {
        public long? OtherUserId { get; set; }
        public MessageDto LastMessage { get; set; }
        public int UnreadCount { get; set; }
    }
}