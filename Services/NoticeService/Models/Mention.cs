namespace NoticeService.Models
{
    public class Mention
    {
        public int UserId { get; set; }
        public string TargetType { get; set; }
        public int TargetId { get; set; }
        public int TargetUserId { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; }
    }
}