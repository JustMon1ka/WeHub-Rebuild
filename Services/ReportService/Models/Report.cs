using Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportService.Models
{
    public enum TargetType
    {
        Post,
        Comment,
        User
    }

    public enum ReportStatus
    {
        Pending,
        Processed,
        Rejected
    }


    [Table("REPORT")]
    public class Report
    {
        [Key]
        [Column("REPORT_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReportId { get; set; }

        [Column("REPORTER_ID")]
        public int ReporterId { get; set; }

        [Column("TARGET_TYPE")]
        [MaxLength(10)]
        public string TargetTypeString { get; set; }

        [NotMapped]
        public TargetType TargetType
        {
            get => Enum.TryParse<TargetType>(TargetTypeString, out var result) ? result : TargetType.Post;
            set => TargetTypeString = value.ToString();
        }

        [Column("TARGET_ID")]
        public int TargetId { get; set; }

        [Column("REASON")]
        [MaxLength(4000)]
        public string? Reason { get; set; }

        [Column("REPORTED_AT")]
        public DateTime ReportedAt { get; set; }

        // 如果需要状态字段，可以考虑在代码层面管理，或者后续添加数据库字段
        [NotMapped]
        public ReportStatus Status { get; set; } = ReportStatus.Pending;  // 代码层面管理状态

        [ForeignKey("ReporterId")]
        public virtual User Reporter { get; set; }
    }
}