using System;

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

    public class Report
    {
        public int ReportId { get; set; }
        public int ReporterId { get; set; }
        public TargetType TargetType { get; set; }
        public int TargetId { get; set; }
        public string Reason { get; set; }
        public DateTime ReportedAt { get; set; }
        public ReportStatus Status { get; set; } = ReportStatus.Pending; // 默认 pending
    }
}