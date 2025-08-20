namespace NoticeService.DTOs
{
    public class RepostNotificationDto
    {
        public int RepostId { get; set; }
        public int UserId { get; set; }
        public int PostId { get; set; }
        public string CommentPreview { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}