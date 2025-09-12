using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NoticeService.Models
{
    [Table("LIKES")]
    public class Like
    {
        [Key]
        [Column("USER_ID", Order = 0)]
        public int UserId { get; set; }

        [Key]
        [Column("TARGET_ID", Order = 1)]
        public int TargetId { get; set; }

        [Column("TARGET_TYPE")]
        [MaxLength(10)]
        public string? TargetType { get; set; }

        [Column("LIKE_TYPE")]
        public int? LikeType { get; set; }

        [Column("LIKE_TIME")]
        public DateTime? LikeTime { get; set; }

        [Column("TARGET_USER_ID")]
        public int? TargetUserId { get; set; }

        [NotMapped]
        public List<int> LikerIds { get; set; } // 临时存储点赞者 ID 摘要
    }
}