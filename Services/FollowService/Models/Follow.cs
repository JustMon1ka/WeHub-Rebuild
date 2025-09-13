using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Models;

namespace FollowService.Models
{
    [Table("FOLLOW")]
    public class Follow
    {
        [Key, Column("FOLLOWER_ID", Order = 0)]
        public int FollowerId { get; set; }

        [Key, Column("FOLLOWEE_ID", Order = 1)]
        public int FolloweeId { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; }

        // [ForeignKey("FollowerId")]
        // public virtual User Follower { get; set; }
        //
        // [ForeignKey("FolloweeId")]
        // public virtual User Followee { get; set; }
    }
}