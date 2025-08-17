using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.JavaScript;
using Models;

namespace Models;

[Table("CIRCLE")]
public class Circle
{
    [Key]
    [Column("CIRCLE_ID")]
    public long CircleId { get; set; }

    [Column("NAME")] 
    public string? Name { get; set; } = string.Empty;

    [Column("DESCRIPTION")] 
    public string? Description { get; set; } = string.Empty;
    
    [Column("OWNER_ID")]
    public long? OwnerId { get; set; }
    
    [Column("CREATED_AT")]
    public DateTime? CreatedAt { get; set; }
    
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}