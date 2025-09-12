namespace PostService.DTOs
{
    public class CheckLikeRequest
    {
        public string Type { get; set; } // "post", "comment", "reply"
        public long TargetId { get; set; }
    }
}