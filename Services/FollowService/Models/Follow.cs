using System;

namespace FollowService.Models
{
    public class Follow
    {
        public int FollowerId { get; set; }
        public int FolloweeId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}