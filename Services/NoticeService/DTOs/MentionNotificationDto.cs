namespace NoticeService.DTOs
{
    public class MentionNotificationDto
    {
        public int TargetId { get; set; }
        public string TargetType { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}