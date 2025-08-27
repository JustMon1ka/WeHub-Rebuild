using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using CircleService.Controllers;
using CircleService.Services;
using CircleService.DTOs;
using DTOs;

namespace CircleService.Tests;

/// <summary>
/// 圈子分类功能的Controller层单元测试（修正版）
/// </summary>
[TestClass]
public class CirclesControllerCategoriesTestsFixed
{
    private Mock<ICircleService> _mockCircleService = null!;
    private Mock<ICircleMemberService> _mockMemberService = null!;
    private Mock<IActivityService> _mockActivityService = null!;
    private CirclesController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockCircleService = new Mock<ICircleService>();
        _mockMemberService = new Mock<ICircleMemberService>();
        _mockActivityService = new Mock<IActivityService>();
        _controller = new CirclesController(
            _mockCircleService.Object, 
            _mockMemberService.Object, 
            _mockActivityService.Object
        );
    }

    [TestMethod]
    public async Task GetAllCircles_WithCategoryFilter_ShouldReturnFilteredCircles()
    {
        // Arrange
        var category = "科技";
        var expectedCircles = new List<CircleDto>
        {
            new CircleDto 
            { 
                CircleId = 1, 
                Name = "技术分享群", 
                Categories = "科技,编程",
                MemberCount = 10,
                OwnerId = 1,
                CreatedAt = DateTime.UtcNow
            },
            new CircleDto 
            { 
                CircleId = 2, 
                Name = "AI研究组", 
                Categories = "科技,人工智能",
                MemberCount = 5,
                OwnerId = 2,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockCircleService.Setup(service => service.GetAllCirclesAsync(null, category, null))
                          .ReturnsAsync(expectedCircles);

        // Act
        var result = await _controller.GetAllCircles(null, category, null);

        // Assert
        Assert.IsInstanceOfType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        
        var response = okResult.Value as BaseHttpResponse<object>;
        Assert.IsNotNull(response);
        Assert.AreEqual(200, response.Code);
        
        var circles = response.Data as IEnumerable<CircleDto>;
        Assert.IsNotNull(circles);
        Assert.AreEqual(2, circles.Count());
        
        _mockCircleService.Verify(service => service.GetAllCirclesAsync(null, category, null), Times.Once);
    }

    [TestMethod]
    public async Task CreateCircle_WithCategories_ShouldCreateSuccessfully()
    {
        // Arrange
        var createDto = new CreateCircleDto
        {
            Name = "新技术圈子",
            Description = "讨论最新技术",
            Categories = "科技,前沿,创新"
        };
        
        var expectedCircle = new CircleDto
        {
            CircleId = 1,
            Name = createDto.Name,
            Description = createDto.Description,
            Categories = createDto.Categories,
            OwnerId = 1,
            MemberCount = 1,
            CreatedAt = DateTime.UtcNow
        };

        _mockCircleService.Setup(service => service.CreateCircleAsync(createDto, It.IsAny<int>()))
                          .ReturnsAsync(expectedCircle);

        // Act
        var result = await _controller.CreateCircle(createDto);

        // Assert
        Assert.IsInstanceOfType<CreatedAtActionResult>(result);
        var createdResult = result as CreatedAtActionResult;
        Assert.IsNotNull(createdResult);
        
        var response = createdResult.Value as BaseHttpResponse<object>;
        Assert.IsNotNull(response);
        Assert.AreEqual(200, response.Code);
        Assert.AreEqual("圈子创建成功", response.Msg);
        
        _mockCircleService.Verify(service => service.CreateCircleAsync(createDto, It.IsAny<int>()), Times.Once);
    }

    [TestMethod]
    public async Task UpdateCircle_WithNewCategories_ShouldUpdateSuccessfully()
    {
        // Arrange
        var circleId = 1;
        var updateDto = new UpdateCircleDto
        {
            Name = "更新后的圈子",
            Description = "更新后的描述",
            Categories = "新分类,更新,测试"
        };

        _mockCircleService.Setup(service => service.UpdateCircleAsync(circleId, updateDto))
                          .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateCircle(circleId, updateDto);

        // Assert
        Assert.IsInstanceOfType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        
        var response = okResult.Value as BaseHttpResponse<object>;
        Assert.IsNotNull(response);
        Assert.AreEqual(200, response.Code);
        Assert.AreEqual("圈子更新成功", response.Msg);
        
        _mockCircleService.Verify(service => service.UpdateCircleAsync(circleId, updateDto), Times.Once);
    }

    [TestMethod]
    public async Task UpdateCircle_CircleNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var circleId = 999; // 不存在的圈子ID
        var updateDto = new UpdateCircleDto
        {
            Name = "更新圈子",
            Categories = "测试"
        };

        _mockCircleService.Setup(service => service.UpdateCircleAsync(circleId, updateDto))
                          .ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateCircle(circleId, updateDto);

        // Assert
        Assert.IsInstanceOfType<NotFoundObjectResult>(result);
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        
        var response = notFoundResult.Value as BaseHttpResponse<object>;
        Assert.IsNotNull(response);
        Assert.AreEqual(404, response.Code);
        Assert.AreEqual("圈子不存在或更新失败", response.Msg);
        
        _mockCircleService.Verify(service => service.UpdateCircleAsync(circleId, updateDto), Times.Once);
    }
}



