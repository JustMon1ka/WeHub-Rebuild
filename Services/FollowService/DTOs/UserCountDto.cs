namespace FollowService.DTOs
{
    public class UserCountDto
    {
        public int FollowingCount { get; set; } // 关注数
        public int FollowerCount { get; set; }  // 被关注数
    }
}