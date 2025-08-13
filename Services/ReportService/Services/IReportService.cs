using System.Collections.Generic;
using System.Threading.Tasks;
using ReportService.DTOs;

namespace ReportService.Services
{
    public interface IReportService
    {
        Task<ReportDto> CreateReportAsync(CreateReportDto createReportDto);
        Task<IEnumerable<ReportDto>> GetReportsAsync();
        Task<ReportDto> UpdateReportAsync(int reportId, UpdateReportDto updateReportDto);
    }
}