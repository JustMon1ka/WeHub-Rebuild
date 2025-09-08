namespace NoticeService.DTOs
{
    public class ReplyNotificationDto
    {
        public int ReplyId { get; set; }
        public int ReplyPoster { get; set; }
        public int CommentId { get; set; }
        public string ContentPreview { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}