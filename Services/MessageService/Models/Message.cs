using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        public int SenderId { get; set; }

        [Column("RECEIVER_ID")]
        public int ReceiverId { get; set; }

        [Column("CONTENT")]
        [MaxLength(4000)]
        public string? Content { get; set; }

        [Column("SENT_AT")]
        public DateTime SentAt { get; set; }

        [Column("IS_READ")]
        public int IsReadNumber { get; set; }

        [NotMapped]
        public bool IsRead
        {
            get => IsReadNumber != 0; // 非0为true，0为false
            set => IsReadNumber = value ? 1 : 0;
        }

        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; }

        [ForeignKey("ReceiverId")]
        public virtual User Receiver { get; set; }
    }
}