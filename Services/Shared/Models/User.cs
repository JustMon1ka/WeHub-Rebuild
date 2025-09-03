using Models;

namespace Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("USERS")]
public class User
{
    [Key]
    [Column("USER_ID")]
    public long? UserId { get; set; }

    [Column("USERNAME")]
    public string? Username { get; set; }

    [Column("PASSWORD_HASH")]
    public string? PasswordHash { get; set; }

    [Column("EMAIL")]
    public string? Email { get; set; }

    [Column("PHONE")]
    public string? Phone { get; set; }

    [Column("CREATED_AT")]
    public DateTime? CreatedAt { get; set; }

    [Column("STATUS")]
    public int? Status { get; set; }
    
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    public virtual ICollection<Comments> Comments { get; set; } = new List<Comments>();
    public virtual ICollection<Reply> Replies { get; set; } = new List<Reply>();
    public virtual UserProfile? UserProfile { get; set; }
}



