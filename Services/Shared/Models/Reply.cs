using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models;

[Table("REPLY")]
public class Reply
{
    [Key]
    [Column("REPLY_ID")]
    public long ReplyId { get; set; }
    [Column("COMMENT_ID")]
    public long CommentId { get; set; }
    [Column("USER_ID")]
    public long UserId { get; set; }
    [Column("CONTENT")]
    public string? Content { get; set; }
    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; }
    [Column("IS_DELETED")]
    public int? IsDeleted { get; set; } 
    
    public virtual User? User { get; set; }
    public virtual Comments? Comment { get; set; }
}