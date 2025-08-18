using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ReportService.DTOs;
using ReportService.Models;
using ReportService.Repositories;
using Microsoft.AspNetCore.Http;

namespace ReportService.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReportService(IReportRepository reportRepository, IHttpContextAccessor httpContextAccessor)
        {
            _reportRepository = reportRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("无法获取当前用户ID");
            }
            return userId;
        }

        private bool IsAdmin()
        {
            return _httpContextAccessor.HttpContext?.User.IsInRole("Admin") ?? false;
        }

        public async Task<ReportDto> CreateReportAsync(CreateReportDto createReportDto)
        {
            int currentUserId = GetCurrentUserId();

            var existingReport = await _reportRepository.GetExistingReportAsync(currentUserId, createReportDto.TargetType, createReportDto.TargetId);
            if (existingReport != null)
            {
                throw new InvalidOperationException("重复举报：您已对该内容提交过举报");
            }

            var report = new Report
            {
                ReporterId = currentUserId,
                TargetType = createReportDto.TargetType,
                TargetId = createReportDto.TargetId,
                Reason = createReportDto.Reason,
                ReportedAt = DateTime.UtcNow,
                Status = ReportStatus.Pending
            };

            var createdReport = await _reportRepository.CreateReportAsync(report);

            return MapToDto(createdReport);
        }

        public async Task<IEnumerable<ReportDto>> GetReportsAsync()
        {
            if (!IsAdmin())
            {
                throw new UnauthorizedAccessException("仅管理员可查询举报记录");
            }

            var reports = await _reportRepository.GetAllReportsAsync();
            return reports.Select(MapToDto);
        }

        public async Task<ReportDto> UpdateReportAsync(int reportId, UpdateReportDto updateReportDto)
        {
            if (!IsAdmin())
            {
                throw new UnauthorizedAccessException("仅管理员可处理举报");
            }

            var report = await _reportRepository.GetReportByIdAsync(reportId);
            if (report == null)
            {
                return null;
            }

            report.Status = updateReportDto.Status;

            await _reportRepository.UpdateReportAsync(report);

            return MapToDto(report);
        }

        private ReportDto MapToDto(Report report)
        {
            return new ReportDto
            {
                ReportId = report.ReportId,
                ReporterId = report.ReporterId,
                TargetType = report.TargetType,
                TargetId = report.TargetId,
                Reason = report.Reason,
                ReportedAt = report.ReportedAt,
                Status = report.Status
            };
        }
    }
}