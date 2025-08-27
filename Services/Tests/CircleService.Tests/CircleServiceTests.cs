namespace CircleService.Tests;

using Moq;
using CircleService.Services;
using CircleService.Repositories;
using CircleService.Models;
using CircleService.DTOs;

[TestClass]
public class CircleServiceTests
{
    private Mock<ICircleRepository> _mockCircleRepo;
    private Mock<ICircleMemberRepository> _mockMemberRepo;
    private Mock<IFileBrowserClient> _mockFileBrowserClient;
    private Services.CircleService _circleService;

    [TestInitialize]
    public void Setup()
    {
        // 在每个测试方法运行前，都会执行这个初始化方法
        _mockCircleRepo = new Mock<ICircleRepository>();
        _mockMemberRepo = new Mock<ICircleMemberRepository>();
        _mockFileBrowserClient = new Mock<IFileBrowserClient>();
        _circleService = new Services.CircleService(_mockCircleRepo.Object, _mockMemberRepo.Object, _mockFileBrowserClient.Object);
    }

    [TestMethod]
    public async Task GetAllCirclesAsync_WithNameFilter_ReturnsFilteredCircles()
    {
        // 1. Arrange (准备阶段)
        var filterName = "Test";
        var allCircles = new List<Circle>
        {
            new Circle { CircleId = 1, Name = "Test Circle 1" },
            new Circle { CircleId = 2, Name = "Another Circle" },
            new Circle { CircleId = 3, Name = "Test Circle 2" }
        };
        
        var expectedCircles = allCircles.Where(c => c.Name.Contains(filterName)).ToList();

        // 设置模拟仓储：当调用 GetAllAsync 并传入 filterName 时，返回我们预设的筛选结果
        _mockCircleRepo.Setup(repo => repo.GetAllAsync(filterName, null, It.IsAny<int?>()))
                       .ReturnsAsync(expectedCircles);

        // 设置模拟成员仓储：因为我们不测试成员数量，所以返回一个空字典即可
        _mockMemberRepo.Setup(repo => repo.GetMemberCountsByCircleIdsAsync(It.IsAny<IEnumerable<int>>()))
                       .ReturnsAsync(new Dictionary<int, int>());

        // 2. Act (执行阶段)
        var result = await _circleService.GetAllCirclesAsync(filterName);
        var resultList = result.ToList();

        // 3. Assert (断言阶段)
        Assert.IsNotNull(result);
        Assert.AreEqual(2, resultList.Count, "筛选结果的数量应为2");
        Assert.IsTrue(resultList.All(c => c.Name.Contains(filterName)), "所有返回的圈子名称都应包含筛选关键字");
        
        // 验证 CircleRepository 的 GetAllAsync 方法确实被带着正确的参数调用了1次
        _mockCircleRepo.Verify(repo => repo.GetAllAsync(filterName, null, It.IsAny<int?>()), Times.Once);
    }
}