namespace PostService.DTOs
{
    public class LikeRequest
    {
        public int UserId { get; set; }    // 点赞用户ID
        public string Type { get; set; }     // post/comment/reply
        public int TargetId { get; set; }
        public bool Like { get; set; }       // true 点赞, false 取消
    }
}
