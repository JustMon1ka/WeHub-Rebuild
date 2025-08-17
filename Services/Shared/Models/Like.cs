using System.ComponentModel.DataAnnotations;

namespace PostService.Models
{
    public class Like
    {
        [Key]
        public int UserId { get; set; }

        [Key]
        public int TargetId { get; set; }

        [Required]
        public string TargetType { get; set; }  // "post", "comment", "reply"

        public bool IsLike { get; set; }
    }
}
