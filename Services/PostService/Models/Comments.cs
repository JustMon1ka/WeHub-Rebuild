using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PostService.Models;

[Table("COMMENTS")]
public class Comments
{
    [Key]
    [Column("COMMENT_ID")]
    public long CommentId { get; set; }
    [Column("POST_ID")]
    public long PostId { get; set; }
    [Column("USER_ID")]
    public long UserId { get; set; }
    [Column("CONTENT")]
    public string? Content { get; set; }
    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; }
    [Column("LIKES")]
    public long Likes { get; set; }
    [Column("IS_DELETED")]
    public int? IsDeleted { get; set; } 
}