using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Models;

namespace UserDataService.Models
{
    [Table("USERPROFILE")]
    public class UserProfile
    {
        [Key]
        [ForeignKey("User")]
        [Column("USER_ID")]
        public int UserId { get; set; }

        [Column("AVATAR_URL")]
        public string? AvatarUrl { get; set; }

        [Column("BIO")]
        public string? Bio { get; set; }

        [Column("GENDER")]
        public string? Gender { get; set; }

        [Column("BIRTHDAY")]
        public DateTime? Birthday { get; set; }

        [Column("LOCATION")]
        public string? Location { get; set; }

        [Column("EXPERIENCE")]
        public int Experience { get; set; }

        [Column("LEVEL")]
        public int Level { get; set; }
        
        [Column("NICKNAME")]
        public string? Nickname { get; set; }
        
        [Column("PROFILE_URL")]
        public string? ProfileUrl { get; set; }

        public User? User { get; set; }
    }
}