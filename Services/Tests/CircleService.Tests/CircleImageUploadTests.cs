using Microsoft.VisualStudio.TestTools.UnitTesting;
using CircleService.Services;
using CircleService.Repositories;
using CircleService.DTOs;
using CircleService.Models;
using System.Text;
using Moq;

namespace CircleService.Tests
{
    [TestClass]
    public class CircleImageUploadTests
    {
        private CircleService.Services.CircleService _circleService;
        private Mock<ICircleRepository> _mockCircleRepository;
        private Mock<IFileBrowserClient> _mockFileBrowserClient;
        private Circle _testCircle;

        [TestInitialize]
        public void Setup()
        {
            _mockCircleRepository = new Mock<ICircleRepository>();
            _mockFileBrowserClient = new Mock<IFileBrowserClient>();
            var mockMemberRepository = new Mock<ICircleMemberRepository>();
            _circleService = new CircleService.Services.CircleService(_mockCircleRepository.Object, mockMemberRepository.Object, _mockFileBrowserClient.Object);

            // 创建测试圈子
            _testCircle = new Circle
            {
                CircleId = 1,
                Name = "测试圈子",
                Description = "测试描述",
                OwnerId = 1,
                CreatedAt = DateTime.UtcNow,
                Categories = "技术,编程",
                AvatarUrl = null,
                BannerUrl = null
            };
        }

        [TestMethod]
        public async Task UploadAvatarAsync_ValidCircleAndFile_ShouldUpdateAvatarUrl()
        {
            // Arrange
            var circleId = 1;
            var fileName = "test_avatar.png";
            var contentType = "image/png";
            var fileContent = Encoding.UTF8.GetBytes("fake image content");
            var stream = new MemoryStream(fileContent);

            _mockCircleRepository.Setup(r => r.GetByIdAsync(circleId))
                .ReturnsAsync(_testCircle);

            _mockFileBrowserClient.Setup(f => f.UploadFileAsync(
                It.IsAny<string>(), 
                contentType, 
                It.IsAny<Stream>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"url\":\"http://120.26.118.70:5001/api/resources/circles/1/avatar.png\"}")
                });

            _mockCircleRepository.Setup(r => r.UpdateAsync(It.IsAny<Circle>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _circleService.UploadAvatarAsync(circleId, stream, fileName, contentType);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ImageUrl.Contains("circles/1/avatar"));
            Assert.AreEqual(fileName, result.FileName);
            Assert.AreEqual(contentType, result.ContentType);
            Assert.AreEqual(fileContent.Length, result.FileSize);

            // 验证圈子被更新
            _mockCircleRepository.Verify(r => r.UpdateAsync(It.Is<Circle>(c => 
                c.CircleId == circleId && 
                !string.IsNullOrEmpty(c.AvatarUrl))), Times.Once);
        }

        [TestMethod]
        public async Task UploadBannerAsync_ValidCircleAndFile_ShouldUpdateBannerUrl()
        {
            // Arrange
            var circleId = 1;
            var fileName = "test_banner.jpg";
            var contentType = "image/jpeg";
            var fileContent = Encoding.UTF8.GetBytes("fake banner content");
            var stream = new MemoryStream(fileContent);

            _mockCircleRepository.Setup(r => r.GetByIdAsync(circleId))
                .ReturnsAsync(_testCircle);

            _mockFileBrowserClient.Setup(f => f.UploadFileAsync(
                It.IsAny<string>(), 
                contentType, 
                It.IsAny<Stream>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"url\":\"http://120.26.118.70:5001/api/resources/circles/1/banner.jpg\"}")
                });

            _mockCircleRepository.Setup(r => r.UpdateAsync(It.IsAny<Circle>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _circleService.UploadBannerAsync(circleId, stream, fileName, contentType);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ImageUrl.Contains("circles/1/banner"));
            Assert.AreEqual(fileName, result.FileName);
            Assert.AreEqual(contentType, result.ContentType);
            Assert.AreEqual(fileContent.Length, result.FileSize);

            // 验证圈子被更新
            _mockCircleRepository.Verify(r => r.UpdateAsync(It.Is<Circle>(c => 
                c.CircleId == circleId && 
                !string.IsNullOrEmpty(c.BannerUrl))), Times.Once);
        }

        [TestMethod]
        public async Task UploadAvatarAsync_CircleNotFound_ShouldReturnNull()
        {
            // Arrange
            var circleId = 999;
            var fileName = "test_avatar.png";
            var contentType = "image/png";
            var stream = new MemoryStream();

            _mockCircleRepository.Setup(r => r.GetByIdAsync(circleId))
                .ReturnsAsync((Circle)null);

            // Act
            var result = await _circleService.UploadAvatarAsync(circleId, stream, fileName, contentType);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task UploadBannerAsync_CircleNotFound_ShouldReturnNull()
        {
            // Arrange
            var circleId = 999;
            var fileName = "test_banner.jpg";
            var contentType = "image/jpeg";
            var stream = new MemoryStream();

            _mockCircleRepository.Setup(r => r.GetByIdAsync(circleId))
                .ReturnsAsync((Circle)null);

            // Act
            var result = await _circleService.UploadBannerAsync(circleId, stream, fileName, contentType);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task UploadAvatarAsync_FileBrowserError_ShouldReturnNull()
        {
            // Arrange
            var circleId = 1;
            var fileName = "test_avatar.png";
            var contentType = "image/png";
            var stream = new MemoryStream();

            _mockCircleRepository.Setup(r => r.GetByIdAsync(circleId))
                .ReturnsAsync(_testCircle);

            _mockFileBrowserClient.Setup(f => f.UploadFileAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<Stream>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError));

            // Act
            var result = await _circleService.UploadAvatarAsync(circleId, stream, fileName, contentType);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task UploadBannerAsync_FileBrowserError_ShouldReturnNull()
        {
            // Arrange
            var circleId = 1;
            var fileName = "test_banner.jpg";
            var contentType = "image/jpeg";
            var stream = new MemoryStream();

            _mockCircleRepository.Setup(r => r.GetByIdAsync(circleId))
                .ReturnsAsync(_testCircle);

            _mockFileBrowserClient.Setup(f => f.UploadFileAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<Stream>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError));

            // Act
            var result = await _circleService.UploadBannerAsync(circleId, stream, fileName, contentType);

            // Assert
            Assert.IsNull(result);
        }
    }
}
