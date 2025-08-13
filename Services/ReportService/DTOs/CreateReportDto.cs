using System.ComponentModel.DataAnnotations;
using ReportService.Models;

namespace ReportService.DTOs
{
    public class CreateReportDto
    {
        [Required(ErrorMessage = "被举报类型是必填项")]
        public TargetType TargetType { get; set; }

        [Required(ErrorMessage = "被举报内容ID是必填项")]
        public int TargetId { get; set; }

        [Required(ErrorMessage = "举报理由是必填项")]
        [StringLength(500, ErrorMessage = "举报理由长度不能超过500字符")]
        public string Reason { get; set; }
    }
}