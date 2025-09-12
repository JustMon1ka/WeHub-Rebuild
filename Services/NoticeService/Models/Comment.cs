using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NoticeService.Models
{
    [Table("COMMENTS")]
    public class Comment
    {
        [Key]
        [Column("COMMENT_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CommentId { get; set; }

        [Column("POST_ID")]
        public int? PostId { get; set; }

        [Column("USER_ID")]
        public int? UserId { get; set; }

        [Column("CONTENT")]
        [MaxLength(4000)]  // 对应CLOB
        public string? Content { get; set; }

        [Column("CREATED_AT")]
        public DateTime? CreatedAt { get; set; }

        [Column("LIKES")]
        public int? Likes { get; set; }

        [Column("DISLIKES")]
        public int? Dislikes { get; set; }

        // 数据库中的NUMBER字段
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