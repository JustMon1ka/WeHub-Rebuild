namespace NoticeService.DTOs
{
    public class CommentNotificationDto
    {
        public int CommentId { get; set; }
        public int UserId { get; set; } // 评论者 ID
        public int PostId { get; set; } // 所属帖子 ID
        public string ContentPreview { get; set; } // 内容摘要
        public DateTime CreatedAt { get; set; }
    }
}