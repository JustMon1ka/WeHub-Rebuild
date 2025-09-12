using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NoticeService.Models
{
    [Table("REPOSTS")]
    public class Repost
    {
        [Key]
        [Column("REPOST_ID")]
        public int RepostId { get; set; }

        [Column("USER_ID")]
        public int? UserId { get; set; }

        [Column("POST_ID")]
        public int? PostId { get; set; }

        [Column("COMMENT")]
        public string Comment { get; set; }

        [Column("CREATED_AT")]
        public DateTime? CreatedAt { get; set; }

        [Column("TARGET_USER_ID")]
        public int? TargetUserId { get; set; }
    }
}