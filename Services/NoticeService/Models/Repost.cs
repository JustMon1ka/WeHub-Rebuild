namespace NoticeService.Models
{
    public class Repost
    {
        public int RepostId { get; set; }
        public int UserId { get; set; }
        public int PostId { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TargetUserId { get; set; }
    }
}