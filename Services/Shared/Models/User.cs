namespace Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("USERS")]
public class User
{
    [Key]
    [Column("USER_ID")]
    public int UserId { get; set; }

    [Column("USERNAME")]
    public string? Username { get; set; }

    [Column("PASSWORD_HASH")]
    public string? PasswordHash { get; set; }

    [Column("EMAIL")]
    public string? Email { get; set; }

    [Column("PHONE")]
    public string? Phone { get; set; }

    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; }

    [Column("STATUS")]
    public int Status { get; set; }
}



