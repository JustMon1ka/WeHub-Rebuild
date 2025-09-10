using System;

namespace FollowService.DTOs
{
    public class FollowDto
    {
        public int FollowerId { get; set; }
        public int FolloweeId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}