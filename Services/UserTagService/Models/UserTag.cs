using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserTagService.Models
{
    [Table("USERTAG")]
    public class UserTag
    {
        [Column("USER_ID")]
        public int UserId { get; set; }
        
        [Column("TAG_ID")]
        public int TagId { get; set; }
        
    }
}