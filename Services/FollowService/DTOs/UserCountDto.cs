namespace FollowService.DTOs
{
    public class UserCountDto
    {
        public int followingCount { get; set; } // 关注数
        public int followerCount { get; set; }  // 被关注数
    }
}