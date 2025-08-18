using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using ReportService.DTOs;
using ReportService.Models;
using ReportService.Repositories;
using ReportService.Services;

namespace ReportService.Tests
{
    public class ReportServiceTests
    {
        private readonly Mock<IReportRepository> _mockRepository;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly ReportService.Services.ReportService _service;

        public ReportServiceTests()
        {
            _mockRepository = new Mock<IReportRepository>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            // 模拟 HTTP 上下文，设置用户ID为1，角色为Admin
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext { User = claimsPrincipal });

            _service = new ReportService.Services.ReportService(_mockRepository.Object, _mockHttpContextAccessor.Object);
        }

        [Fact]
        public async Task CreateReportAsync_SuccessfulCreate_ReturnsReportDto()
        {
            // Arrange
            var dto = new CreateReportDto { TargetType = TargetType.Post, TargetId = 123, Reason = "违规" };
            _mockRepository.Setup(r => r.GetExistingReportAsync(1, TargetType.Post, 123)).ReturnsAsync((Report)null);
            _mockRepository.Setup(r => r.CreateReportAsync(It.IsAny<Report>())).ReturnsAsync(new Report
            {
                ReportId = 1,
                ReporterId = 1,
                TargetType = TargetType.Post,
                TargetId = 123,
                Reason = "违规",
                ReportedAt = DateTime.UtcNow,
                Status = ReportStatus.Pending
            });

            // Act
            var result = await _service.CreateReportAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ReportId);
            Assert.Equal("违规", result.Reason);
            _mockRepository.Verify(r => r.CreateReportAsync(It.IsAny<Report>()), Times.Once);
        }

        [Fact]
        public async Task CreateReportAsync_DuplicateReport_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new CreateReportDto { TargetType = TargetType.Post, TargetId = 123, Reason = "违规" };
            _mockRepository.Setup(r => r.GetExistingReportAsync(1, TargetType.Post, 123)).ReturnsAsync(new Report { ReportId = 1 });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateReportAsync(dto));
        }

        [Fact]
        public async Task GetReportsAsync_AsAdmin_ReturnsReportDtos()
        {
            // Arrange
            var reports = new List<Report>
            {
                new Report { ReportId = 1, ReporterId = 1, TargetType = TargetType.Post, TargetId = 123, Reason = "违规", ReportedAt = DateTime.UtcNow, Status = ReportStatus.Pending }
            };
            _mockRepository.Setup(r => r.GetAllReportsAsync()).ReturnsAsync(reports);

            // Act
            var result = await _service.GetReportsAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result.First().ReportId);
        }

        [Fact]
        public async Task GetReportsAsync_NotAdmin_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") }; // 无 Admin 角色
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext { User = new ClaimsPrincipal(identity) });

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetReportsAsync());
        }

        [Fact]
        public async Task UpdateReportAsync_AsAdmin_SuccessfulUpdate_ReturnsReportDto()
        {
            // Arrange
            var dto = new UpdateReportDto { Status = ReportStatus.Processed };
            var report = new Report { ReportId = 1, Status = ReportStatus.Pending };
            _mockRepository.Setup(r => r.GetReportByIdAsync(1)).ReturnsAsync(report);
            _mockRepository.Setup(r => r.UpdateReportAsync(It.IsAny<Report>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateReportAsync(1, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ReportStatus.Processed, result.Status);
            _mockRepository.Verify(r => r.UpdateReportAsync(It.Is<Report>(r => r.Status == ReportStatus.Processed)), Times.Once);
        }

        [Fact]
        public async Task UpdateReportAsync_ReportNotFound_ReturnsNull()
        {
            // Arrange
            var dto = new UpdateReportDto { Status = ReportStatus.Processed };
            _mockRepository.Setup(r => r.GetReportByIdAsync(1)).ReturnsAsync((Report)null);

            // Act
            var result = await _service.UpdateReportAsync(1, dto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateReportAsync_NotAdmin_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") }; // 无 Admin 角色
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext { User = new ClaimsPrincipal(identity) });
            var dto = new UpdateReportDto { Status = ReportStatus.Processed };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.UpdateReportAsync(1, dto));
        }

        [Fact]
        public async Task CreateReportAsync_Unauthorized_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext)null); // 模拟未认证
            var dto = new CreateReportDto { TargetType = TargetType.Post, TargetId = 123, Reason = "违规" };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.CreateReportAsync(dto));
        }
    }
}