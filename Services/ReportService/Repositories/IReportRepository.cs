using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using ReportService.Models;

namespace ReportService.Repositories
{
    public interface IReportRepository
    {
        Task<Report> CreateReportAsync(Report report);
        Task<IEnumerable<Report>> GetAllReportsAsync();
        Task<Report> GetReportByIdAsync(int reportId);
        Task<Report> GetExistingReportAsync(int reporterId, TargetType targetType, int targetId);
        Task UpdateReportAsync(Report report);
    }
}