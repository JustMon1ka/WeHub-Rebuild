using System.ComponentModel.DataAnnotations;

namespace PostService.Models
{
    public class Favorite
    {
        [Key]
        public int UserId { get; set; }

        [Key]
        public int PostId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
