using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PostService.Models;

[Table("POST")]
public class Post
{
    [Key]
    [Column("POST_ID")]
    public long PostId { get; set; }

    [Column("USER_ID")]
    public long? UserId { get; set; }

    [Column("CONTENT")]
    public string? Content { get; set; }

    [Column("TITLE")] 
    public string? Title { get; set; }

    [Column("CREATED_AT")]
    public DateTime? CreatedAt { get; set; }

    [Column("IS_DELETED")]
    public int IsDeleted { get; set; }

    [Column("IS_HIDDEN")]
    public int IsHidden { get; set; }

    [Column("VIEWS")]
    public int? Views { get; set; }

    [Column("LIKES")]
    public int? Likes { get; set; }

    [Column("DISLIKES")]
    public int? Dislikes { get; set; }

    // 可选：导航属性
    // public virtual User? User { get; set; }
}