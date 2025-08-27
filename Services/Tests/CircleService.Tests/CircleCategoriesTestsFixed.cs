using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CircleService.Services;
using CircleService.Repositories;
using CircleService.Models;
using CircleService.DTOs;

namespace CircleService.Tests;

/// <summary>
/// 圈子分类功能的单元测试（修正版）
/// </summary>
[TestClass]
public class CircleCategoriesTestsFixed
{
    private Mock<ICircleRepository> _mockCircleRepo = null!;
    private Mock<ICircleMemberRepository> _mockMemberRepo = null!;
    private Mock<IFileBrowserClient> _mockFileBrowserClient = null!;
    private Services.CircleService _circleService = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockCircleRepo = new Mock<ICircleRepository>();
        _mockMemberRepo = new Mock<ICircleMemberRepository>();
        _mockFileBrowserClient = new Mock<IFileBrowserClient>();
        _circleService = new Services.CircleService(_mockCircleRepo.Object, _mockMemberRepo.Object, _mockFileBrowserClient.Object);
    }

    [TestMethod]
    public async Task CreateCircleAsync_WithCategories_ShouldSaveCategories()
    {
        // Arrange
        var createDto = new CreateCircleDto
        {
            Name = "技术交流群",
            Description = "分享最新技术动态",
            Categories = "科技,编程,学习"
        };
        var ownerId = 1;

        _mockCircleRepo.Setup(repo => repo.AddAsync(It.IsAny<Circle>()))
                       .Callback<Circle>(circle => 
                       {
                           circle.CircleId = 1; // 模拟数据库生成ID
                       });

        // Act
        var result = await _circleService.CreateCircleAsync(createDto, ownerId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(createDto.Name, result.Name);
        Assert.AreEqual(createDto.Categories, result.Categories);
        
        // 验证仓储方法被正确调用
        _mockCircleRepo.Verify(repo => repo.AddAsync(It.Is<Circle>(c => 
            c.Name == createDto.Name && 
            c.Categories == createDto.Categories
        )), Times.Once);
    }

    [TestMethod]
    public async Task GetAllCirclesAsync_WithCategoryFilter_ShouldReturnFilteredCircles()
    {
        // Arrange
        var categoryFilter = "科技";
        var allCircles = new List<Circle>
        {
            new Circle { CircleId = 1, Name = "技术分享", Categories = "科技,编程", OwnerId = 1 },
            new Circle { CircleId = 2, Name = "美食爱好", Categories = "生活,美食", OwnerId = 2 },
            new Circle { CircleId = 3, Name = "AI研究", Categories = "科技,人工智能", OwnerId = 1 }
        };
        
        var expectedCircles = allCircles.Where(c => c.Categories != null && c.Categories.Contains(categoryFilter)).ToList();

        _mockCircleRepo.Setup(repo => repo.GetAllAsync(null, categoryFilter, null))
                       .ReturnsAsync(expectedCircles);
        
        _mockMemberRepo.Setup(repo => repo.GetMemberCountsByCircleIdsAsync(It.IsAny<IEnumerable<int>>()))
                       .ReturnsAsync(new Dictionary<int, int> { { 1, 5 }, { 3, 8 } });

        // Act
        var result = await _circleService.GetAllCirclesAsync(null, categoryFilter, null);
        var resultList = result.ToList();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, resultList.Count, "应该返回2个包含'科技'分类的圈子");
        Assert.IsTrue(resultList.All(c => c.Categories != null && c.Categories.Contains(categoryFilter)), 
                     "所有返回的圈子都应包含指定分类");
        
        _mockCircleRepo.Verify(repo => repo.GetAllAsync(null, categoryFilter, null), Times.Once);
    }

    [TestMethod]
    public async Task UpdateCircleAsync_WithNewCategories_ShouldUpdateCategories()
    {
        // Arrange
        var circleId = 1;
        var existingCircle = new Circle
        {
            CircleId = circleId,
            Name = "技术分享",
            Description = "原始描述",
            Categories = "科技",
            OwnerId = 1
        };
        
        var updateDto = new UpdateCircleDto
        {
            Name = "高级技术分享",
            Description = "更新后的描述",
            Categories = "科技,编程,高级"
        };

        _mockCircleRepo.Setup(repo => repo.GetByIdAsync(circleId))
                       .ReturnsAsync(existingCircle);

        // Act
        var result = await _circleService.UpdateCircleAsync(circleId, updateDto);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(updateDto.Name, existingCircle.Name);
        Assert.AreEqual(updateDto.Description, existingCircle.Description);
        Assert.AreEqual(updateDto.Categories, existingCircle.Categories);
        
        _mockCircleRepo.Verify(repo => repo.UpdateAsync(existingCircle), Times.Once);
    }
}



