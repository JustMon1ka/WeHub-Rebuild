namespace NoticeService.Models
{
    public class Reply
    {
        public int ReplyId { get; set; }
        public int CommentId { get; set; }
        public int ReplyPoster { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public int TargetUserId { get; set; }
    }
}