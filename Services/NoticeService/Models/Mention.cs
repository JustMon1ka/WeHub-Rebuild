using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NoticeService.Models
{
    [Table("MENTIONS")]
    public class Mention
    {
        [Key, Column("USER_ID", Order = 0)]
        public int UserId { get; set; }

        [Key, Column("TARGET_TYPE", Order = 1)]
        [MaxLength(50)]
        public string TargetType { get; set; }

        [Key, Column("TARGET_ID", Order = 2)]
        public int TargetId { get; set; }

        [Key, Column("TARGET_USER_ID", Order = 3)]
        public int TargetUserId { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; }
    }
}