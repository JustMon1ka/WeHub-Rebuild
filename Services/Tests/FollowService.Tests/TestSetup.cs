using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using FollowService.DTOs;
using FollowService.Models;
using FollowService.Repositories;
using FollowService.Services;

namespace FollowService.Tests
{
    public class FollowServiceTests
    {
        private readonly Mock<IFollowRepository> _mockRepository;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly FollowService.Services.FollowService _service;

        public FollowServiceTests()
        {
            _mockRepository = new Mock<IFollowRepository>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            // 模拟 HTTP 上下文，设置当前用户ID为1
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext { User = claimsPrincipal });

            _service = new FollowService.Services.FollowService(_mockRepository.Object, _mockHttpContextAccessor.Object);
        }

        [Fact]
        public async Task FollowAsync_SuccessfulFollow_ReturnsFollowDto()
        {
            // Arrange
            var dto = new CreateFollowDto { FolloweeId = 2 };
            _mockRepository.Setup(r => r.GetFollowAsync(1, 2)).ReturnsAsync((Follow)null);
            _mockRepository.Setup(r => r.CreateFollowAsync(It.IsAny<Follow>())).ReturnsAsync( new Follow
            {
                FollowerId = 1,
                FolloweeId = 2,
                CreatedAt = DateTime.UtcNow
            });

            // Act
            var result = await _service.FollowAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.FollowerId);
            Assert.Equal(2, result.FolloweeId);
            _mockRepository.Verify(r => r.CreateFollowAsync(It.IsAny<Follow>()), Times.Once);
        }

        [Fact]
        public async Task FollowAsync_SelfFollow_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new CreateFollowDto { FolloweeId = 1 };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.FollowAsync(dto));
        }

        [Fact]
        public async Task FollowAsync_ExistingFollow_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new CreateFollowDto { FolloweeId = 2 };
            _mockRepository.Setup(r => r.GetFollowAsync(1, 2)).ReturnsAsync(new Follow { FollowerId = 1, FolloweeId = 2 });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.FollowAsync(dto));
        }

        [Fact]
        public async Task UnfollowAsync_ExistingFollow_ReturnsTrue()
        {
            // Arrange
            _mockRepository.Setup(r => r.DeleteFollowAsync(1, 2)).ReturnsAsync(true);

            // Act
            var result = await _service.UnfollowAsync(2);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(r => r.DeleteFollowAsync(1, 2), Times.Once);
        }

        [Fact]
        public async Task UnfollowAsync_NonExistingFollow_ReturnsFalse()
        {
            // Arrange
            _mockRepository.Setup(r => r.DeleteFollowAsync(1, 2)).ReturnsAsync(false);

            // Act
            var result = await _service.UnfollowAsync(2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task FollowAsync_Unauthorized_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext)null); // 模拟未认证
            var dto = new CreateFollowDto { FolloweeId = 2 };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.FollowAsync(dto));
        }
    }
}