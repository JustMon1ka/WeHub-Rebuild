namespace MessageService.DTOs
{
    public class ConversationDto
    {
        public int OtherUserId { get; set; }
        public MessageDto LastMessage { get; set; }
        public int UnreadCount { get; set; }
    }
}