using Microsoft.EntityFrameworkCore;
using ReportService.Data;
using ReportService.Models;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;

namespace ReportService.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly ReportDbContext _context;

        public ReportRepository(ReportDbContext context)
        {
            _context = context;
        }

        public async Task<Report> CreateReportAsync(Report report)
        {
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<IEnumerable<Report>> GetAllReportsAsync()
        {
            return await _context.Reports.ToListAsync();
        }

        public async Task<Report> GetReportByIdAsync(int reportId)
        {
            return await _context.Reports.FindAsync(reportId);
        }

        public async Task<Report> GetExistingReportAsync(int reporterId, TargetType targetType, int targetId)
        {
            return await _context.Reports
                .FirstOrDefaultAsync(r => r.ReporterId == reporterId && r.TargetType == targetType && r.TargetId == targetId);
        }

        public async Task UpdateReportAsync(Report report)
        {
            _context.Reports.Update(report);
            await _context.SaveChangesAsync();
        }
    }
}