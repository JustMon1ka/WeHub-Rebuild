namespace NoticeService.DTOs
{
    public class LikeNotificationDto
    {
        public int TargetId { get; set; }
        public string TargetType { get; set; }
        public DateTime LastLikedAt { get; set; }
        public int LikeCount { get; set; }
        public List<int> LikerIds { get; set; } 
    }
}