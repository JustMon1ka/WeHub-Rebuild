using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Models;

namespace MessageService.Models
{
    [Table("MESSAGE")]
    public class Message
    {
        [Key]
        [Column("MESSAGE_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MessageId { get; set; }

        [Column("SENDER_ID")]
        public long? SenderId { get; set; }

        [Column("RECEIVER_ID")]
        public long? ReceiverId { get; set; }

        [Column("CONTENT")]
        [MaxLength(4000)]
        public string? Content { get; set; }

        [Column("SENT_AT")]
        public DateTime SentAt { get; set; }

        [Column("IS_READ")]
        public bool IsRead { get; set; }

        // [ForeignKey("SenderId")]
        // public virtual User Sender { get; set; }

        // [ForeignKey("ReceiverId")]
        // public virtual User Receiver { get; set; }
    }
}