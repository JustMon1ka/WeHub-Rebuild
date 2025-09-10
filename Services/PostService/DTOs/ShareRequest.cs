namespace PostService.DTOs
{
    public class ShareRequest
    {
        public long TargetId { get; set; }      // 被分享的帖子ID
        public string? Comment { get; set; }    // 用户写的转发附言
        public long? CircleId { get; set; }     // 圈子ID（可选）
    }
}
