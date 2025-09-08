namespace NoticeService.Models
{
    public class Like
    {
        public int UserId { get; set; }
        public string TargetType { get; set; }
        public int TargetId { get; set; }
        public int TargetUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<int> LikerIds { get; set; } // 临时存储点赞者 ID 摘要
    }
}