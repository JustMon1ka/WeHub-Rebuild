using System.ComponentModel.DataAnnotations;
using ReportService.Models;

namespace ReportService.DTOs
{
    public class UpdateReportDto
    {
        [Required(ErrorMessage = "处理状态是必填项")]
        public ReportStatus Status { get; set; }

        public string? AdditionalNotes { get; set; } // 可选额外备注
    }
}