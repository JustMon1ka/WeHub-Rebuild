using System;
using ReportService.Models;

namespace ReportService.DTOs
{
    public class ReportDto
    {
        public int ReportId { get; set; }
        public int ReporterId { get; set; }
        public TargetType TargetType { get; set; }
        public int TargetId { get; set; }
        public string Reason { get; set; }
        public DateTime ReportedAt { get; set; }
        public ReportStatus Status { get; set; }
    }
}