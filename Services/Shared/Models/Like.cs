using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PostService.Models
{
    [Table("LIKES")]
    public class Like
    {
        [Key]
        [Column("USER_ID", Order = 0)]
        public long UserId { get; set; }

        [Key]
        [Column("TARGET_TYPE", Order = 1)]
        [Required]
        public string TargetType { get; set; } = string.Empty;  // "post", "comment", "reply"

        [Key]
        [Column("TARGET_ID", Order = 2)]
        public int TargetId { get; set; }

        [Column("LIKE_TYPE")]
        public int? LikeType { get; set; }

        [Column("LIKE_TIME")]
        public DateTime? LikeTime { get; set; } = DateTime.UtcNow;

        [Column("TARGET_USER_ID")]
        public long? TargetUserId { get; set; }

        [NotMapped]
        public bool IsLike { get; set; }

        [NotMapped]
        public DateTime CreatedAt
        {
            get => LikeTime ?? DateTime.UtcNow;
            set => LikeTime = value;
        }
    }
}
