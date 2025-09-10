using Microsoft.VisualStudio.TestTools.UnitTesting;
using CircleService.Controllers;
using CircleService.Services;
using CircleService.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Text;
using DTOs;
using System.Security.Claims;

namespace CircleService.Tests
{
    [TestClass]
    public class CirclesControllerImageUploadTests
    {
        private CirclesController _controller;
        private Mock<ICircleService> _mockCircleService;
        private Mock<ICircleMemberService> _mockMemberService;
        private Mock<IActivityService> _mockActivityService;

        [TestInitialize]
        public void Setup()
        {
            _mockCircleService = new Mock<ICircleService>();
            _mockMemberService = new Mock<ICircleMemberService>();
            _mockActivityService = new Mock<IActivityService>();
            _controller = new CirclesController(_mockCircleService.Object, _mockMemberService.Object, _mockActivityService.Object);
            
            // 设置默认的认证上下文
            SetupAuthenticationContext("1");
        }

        /// <summary>
        /// 设置测试中的认证上下文
        /// </summary>
        /// <param name="userId">用户ID</param>
        private void SetupAuthenticationContext(string userId)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [TestMethod]
        public async Task UploadAvatar_ValidFile_ShouldReturnSuccess()
        {
            // Arrange
            var circleId = 1;
            var fileName = "test_avatar.png";
            var contentType = "image/png";
            var fileContent = Encoding.UTF8.GetBytes("fake image content");
            var stream = new MemoryStream(fileContent);
            
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.ContentType).Returns(contentType);
            mockFile.Setup(f => f.Length).Returns(fileContent.Length);
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);

            var expectedResponse = new ImageUploadResponseDto
            {
                ImageUrl = "http://120.26.118.70:5001/api/resources/circles/1/avatar.png",
                FileName = fileName,
                ContentType = contentType,
                FileSize = fileContent.Length
            };

            _mockCircleService.Setup(s => s.GetCircleByIdAsync(circleId))
                .ReturnsAsync(new CircleDto { CircleId = circleId, Name = "测试圈子" });

            _mockCircleService.Setup(s => s.UploadAvatarAsync(
                circleId, 
                It.IsAny<Stream>(), 
                fileName, 
                contentType))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UploadAvatar(circleId, mockFile.Object);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            var response = okResult.Value as BaseHttpResponse<object>;
            
            Assert.IsNotNull(response);
            Assert.AreEqual(200, response.Code);
            Assert.AreEqual(expectedResponse.ImageUrl, (response.Data as ImageUploadResponseDto)?.ImageUrl);
        }

        [TestMethod]
        public async Task UploadAvatar_NoFile_ShouldReturnBadRequest()
        {
            // Arrange
            var circleId = 1;
            IFormFile? file = null;

            _mockCircleService.Setup(s => s.GetCircleByIdAsync(circleId))
                .ReturnsAsync(new CircleDto { CircleId = circleId, Name = "测试圈子" });

            // Act
            var result = await _controller.UploadAvatar(circleId, file);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            var response = badRequestResult.Value as BaseHttpResponse<object>;
            
            Assert.IsNotNull(response);
            Assert.AreEqual(400, response.Code);
            Assert.IsTrue(response.Msg?.Contains("请选择要上传的图片文件") == true);
        }

        [TestMethod]
        public async Task UploadAvatar_InvalidFileType_ShouldReturnBadRequest()
        {
            // Arrange
            var circleId = 1;
            var fileName = "test.txt";
            var contentType = "text/plain";
            
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.ContentType).Returns(contentType);
            mockFile.Setup(f => f.Length).Returns(1024);

            _mockCircleService.Setup(s => s.GetCircleByIdAsync(circleId))
                .ReturnsAsync(new CircleDto { CircleId = circleId, Name = "测试圈子" });

            // Act
            var result = await _controller.UploadAvatar(circleId, mockFile.Object);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            var response = badRequestResult.Value as BaseHttpResponse<object>;
            
            Assert.IsNotNull(response);
            Assert.AreEqual(400, response.Code);
            Assert.IsTrue(response.Msg?.Contains("只支持 JPG、PNG、GIF 格式的图片") == true);
        }

        [TestMethod]
        public async Task UploadAvatar_FileTooLarge_ShouldReturnBadRequest()
        {
            // Arrange
            var circleId = 1;
            var fileName = "large_image.png";
            var contentType = "image/png";
            
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.ContentType).Returns(contentType);
            mockFile.Setup(f => f.Length).Returns(6 * 1024 * 1024); // 6MB

            _mockCircleService.Setup(s => s.GetCircleByIdAsync(circleId))
                .ReturnsAsync(new CircleDto { CircleId = circleId, Name = "测试圈子" });

            // Act
            var result = await _controller.UploadAvatar(circleId, mockFile.Object);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            var response = badRequestResult.Value as BaseHttpResponse<object>;
            
            Assert.IsNotNull(response);
            Assert.AreEqual(400, response.Code);
            Assert.IsTrue(response.Msg?.Contains("图片文件大小不能超过5MB") == true);
        }

        [TestMethod]
        public async Task UploadAvatar_CircleNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var circleId = 999;
            var fileName = "test_avatar.png";
            var contentType = "image/png";
            
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.ContentType).Returns(contentType);
            mockFile.Setup(f => f.Length).Returns(1024);

            _mockCircleService.Setup(s => s.GetCircleByIdAsync(circleId))
                .ReturnsAsync((CircleDto)null);

            // Act
            var result = await _controller.UploadAvatar(circleId, mockFile.Object);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFoundResult = result as NotFoundObjectResult;
            var response = notFoundResult.Value as BaseHttpResponse<object>;
            
            Assert.IsNotNull(response);
            Assert.AreEqual(404, response.Code);
            Assert.IsTrue(response.Msg?.Contains("圈子ID 999 不存在") == true);
        }

        [TestMethod]
        public async Task UploadAvatar_ServiceReturnsNull_ShouldReturnNotFound()
        {
            // Arrange
            var circleId = 1;
            var fileName = "test_avatar.png";
            var contentType = "image/png";
            var fileContent = Encoding.UTF8.GetBytes("fake image content");
            var stream = new MemoryStream(fileContent);
            
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.ContentType).Returns(contentType);
            mockFile.Setup(f => f.Length).Returns(fileContent.Length);
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);

            _mockCircleService.Setup(s => s.GetCircleByIdAsync(circleId))
                .ReturnsAsync(new CircleDto { CircleId = circleId, Name = "测试圈子" });

            _mockCircleService.Setup(s => s.UploadAvatarAsync(
                circleId, 
                It.IsAny<Stream>(), 
                fileName, 
                contentType))
                .ReturnsAsync((ImageUploadResponseDto)null);

            // Act
            var result = await _controller.UploadAvatar(circleId, mockFile.Object);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFoundResult = result as NotFoundObjectResult;
            var response = notFoundResult.Value as BaseHttpResponse<object>;
            
            Assert.IsNotNull(response);
            Assert.AreEqual(404, response.Code);
            Assert.IsTrue(response.Msg?.Contains("FileBrowser上传失败") == true);
        }
    }
}
