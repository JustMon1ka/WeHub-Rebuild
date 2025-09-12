using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PostService.Models
{
    [Table("Likes")]
    public class Like
    {
        public long UserId { get; set; }

        public int TargetId { get; set; }

        [Required]
        public string TargetType { get; set; }  // "post", "comment", "reply"

        public bool IsLike { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
