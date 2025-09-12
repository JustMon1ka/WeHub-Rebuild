using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NoticeService.Models
{
    [Table("REPLY")]
    public class Reply
    {
        [Key]
        [Column("REPLY_ID")]
        public int ReplyId { get; set; }

        [Column("COMMENT_ID")]
        public int? CommentId { get; set; }

        [Column("USER_ID")]
        public int? ReplyPoster { get; set; }

        [Column("CONTENT")]
        public string Content { get; set; }

        [Column("CREATED_AT")]
        public DateTime? CreatedAt { get; set; }

        [Column("IS_DELETED")]
        public int IsDeletedNumber { get; set; }

        [NotMapped]
        public bool IsDeleted
        {
            get => IsDeletedNumber != 0;
            set => IsDeletedNumber = value ? 1 : 0;
        }

        [Column("TARGET_USER_ID")]
        public int? TargetUserId { get; set; }
    }
}