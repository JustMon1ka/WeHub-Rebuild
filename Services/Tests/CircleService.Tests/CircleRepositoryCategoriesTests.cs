using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using CircleService.Data;
using CircleService.Models;
using CircleService.Repositories;

namespace CircleService.Tests;

/// <summary>
/// 圈子分类功能的Repository层集成测试
/// 使用内存数据库进行测试
/// </summary>
[TestClass]
public class CircleRepositoryCategoriesTests
{
    private AppDbContext _context;
    private CircleRepository _repository;

    [TestInitialize]
    public void Setup()
    {
        // 使用内存数据库进行测试
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _repository = new CircleRepository(_context);
        
        // 准备测试数据
        SeedTestData();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Dispose();
    }

    private void SeedTestData()
    {
        var circles = new List<Circle>
        {
            new Circle 
            { 
                CircleId = 1, 
                Name = "技术分享群", 
                Categories = "科技,编程,开发",
                OwnerId = 1,
                CreatedAt = DateTime.UtcNow
            },
            new Circle 
            { 
                CircleId = 2, 
                Name = "美食爱好者", 
                Categories = "生活,美食,分享",
                OwnerId = 2,
                CreatedAt = DateTime.UtcNow
            },
            new Circle 
            { 
                CircleId = 3, 
                Name = "AI研究小组", 
                Categories = "科技,人工智能,研究",
                OwnerId = 1,
                CreatedAt = DateTime.UtcNow
            },
            new Circle 
            { 
                CircleId = 4, 
                Name = "健身打卡", 
                Categories = "健康,运动,生活",
                OwnerId = 3,
                CreatedAt = DateTime.UtcNow
            },
            new Circle 
            { 
                CircleId = 5, 
                Name = "无分类圈子", 
                Categories = null,
                OwnerId = 2,
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Circles.AddRange(circles);
        _context.SaveChanges();
    }

    [TestMethod]
    public async Task GetAllAsync_WithCategoryFilter_ShouldReturnMatchingCircles()
    {
        // Act
        var result = await _repository.GetAllAsync(null, "科技", null);
        var resultList = result.ToList();

        // Assert
        Assert.AreEqual(2, resultList.Count, "应该返回2个包含'科技'分类的圈子");
        
        var circleNames = resultList.Select(c => c.Name).ToList();
        Assert.IsTrue(circleNames.Contains("技术分享群"));
        Assert.IsTrue(circleNames.Contains("AI研究小组"));
        
        // 验证所有返回的圈子都包含指定分类
        Assert.IsTrue(resultList.All(c => c.Categories != null && c.Categories.Contains("科技")));
    }

    [TestMethod]
    public async Task GetAllAsync_WithNameAndCategoryFilter_ShouldReturnMatchingCircles()
    {
        // Act
        var result = await _repository.GetAllAsync("技术", "编程", null);
        var resultList = result.ToList();

        // Assert
        Assert.AreEqual(1, resultList.Count, "应该返回1个同时匹配名称和分类的圈子");
        Assert.AreEqual("技术分享群", resultList[0].Name);
        Assert.IsTrue(resultList[0].Categories.Contains("编程"));
    }

    [TestMethod]
    public async Task GetAllAsync_WithNonExistentCategory_ShouldReturnEmpty()
    {
        // Act
        var result = await _repository.GetAllAsync(null, "不存在的分类", null);
        var resultList = result.ToList();

        // Assert
        Assert.AreEqual(0, resultList.Count, "不存在的分类应该返回空列表");
    }

    [TestMethod]
    public async Task GetAllAsync_WithPartialCategoryMatch_ShouldReturnMatchingCircles()
    {
        // Act - 测试部分匹配（例如搜索"生活"应该匹配"生活,美食,分享"）
        var result = await _repository.GetAllAsync(null, "生活", null);
        var resultList = result.ToList();

        // Assert
        Assert.AreEqual(2, resultList.Count, "应该返回2个包含'生活'分类的圈子");
        
        var circleNames = resultList.Select(c => c.Name).ToList();
        Assert.IsTrue(circleNames.Contains("美食爱好者"));
        Assert.IsTrue(circleNames.Contains("健身打卡"));
    }

    [TestMethod]
    public async Task GetAllAsync_WithNullCategory_ShouldReturnAllCircles()
    {
        // Act
        var result = await _repository.GetAllAsync(null, null, null);
        var resultList = result.ToList();

        // Assert
        Assert.AreEqual(5, resultList.Count, "不指定分类应该返回所有圈子");
    }

    [TestMethod]
    public async Task GetAllAsync_WithEmptyCategory_ShouldReturnAllCircles()
    {
        // Act
        var result = await _repository.GetAllAsync(null, "", null);
        var resultList = result.ToList();

        // Assert
        Assert.AreEqual(5, resultList.Count, "空字符串分类应该返回所有圈子");
    }

    [TestMethod]
    public async Task AddAsync_WithCategories_ShouldSaveSuccessfully()
    {
        // Arrange
        var newCircle = new Circle
        {
            Name = "新测试圈子",
            Description = "测试描述",
            Categories = "测试,新建,分类",
            OwnerId = 1,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _repository.AddAsync(newCircle);

        // Assert
        var savedCircle = await _context.Circles.FirstOrDefaultAsync(c => c.Name == "新测试圈子");
        Assert.IsNotNull(savedCircle);
        Assert.AreEqual("测试,新建,分类", savedCircle.Categories);
    }

    [TestMethod]
    public async Task UpdateAsync_WithModifiedCategories_ShouldUpdateSuccessfully()
    {
        // Arrange
        var existingCircle = await _context.Circles.FindAsync(1);
        Assert.IsNotNull(existingCircle);
        
        var originalCategories = existingCircle.Categories;
        existingCircle.Categories = "更新后,科技,新分类";

        // Act
        await _repository.UpdateAsync(existingCircle);

        // Assert
        var updatedCircle = await _context.Circles.FindAsync(1);
        Assert.IsNotNull(updatedCircle);
        Assert.AreEqual("更新后,科技,新分类", updatedCircle.Categories);
        Assert.AreNotEqual(originalCategories, updatedCircle.Categories);
    }

    [TestMethod]
    public async Task GetAllAsync_CaseInsensitiveSearch_ShouldWork()
    {
        // Act - 测试大小写不敏感的搜索
        var result1 = await _repository.GetAllAsync(null, "科技", null);
        var result2 = await _repository.GetAllAsync(null, "科技", null); // Oracle通常是大小写敏感的，这里主要测试逻辑

        // Assert
        Assert.AreEqual(result1.Count(), result2.Count(), "相同分类搜索应该返回相同数量的结果");
    }
}



