using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CircleService.Services;
using CircleService.Repositories;
using CircleService.Models;
using CircleService.DTOs;
using System.Threading.Tasks;

namespace CircleService.Tests;

[TestClass]
public class ActivityServiceTests
{
    private Mock<IActivityRepository> _mockActivityRepository;
    private Mock<ICircleRepository> _mockCircleRepository;
    private Mock<ICircleMemberRepository> _mockMemberRepository;
    private Mock<IActivityParticipantRepository> _mockParticipantRepository;
    private ActivityService _activityService;

    [TestInitialize]
    public void Setup()
    {
        _mockActivityRepository = new Mock<IActivityRepository>();
        _mockCircleRepository = new Mock<ICircleRepository>();
        _mockMemberRepository = new Mock<ICircleMemberRepository>();
        _mockParticipantRepository = new Mock<IActivityParticipantRepository>();
        _activityService = new ActivityService(
            _mockActivityRepository.Object,
            _mockCircleRepository.Object,
            _mockMemberRepository.Object,
            _mockParticipantRepository.Object
        );
    }

    [TestMethod]
    public async Task CreateActivityAsync_CircleNotFound_ShouldFail()
    {
        // Arrange
        var circleId = 1;
        var createActivityDto = new CreateActivityDto
        {
            Title = "Test Activity",
            Description = "Test Description",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(1)
        };
        var creatorId = 1;

        _mockCircleRepository.Setup(repo => repo.GetByIdAsync(circleId)).ReturnsAsync((Circle)null);

        // Act
        var result = await _activityService.CreateActivityAsync(circleId, createActivityDto, creatorId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("圈子不存在。", result.ErrorMessage);
    }

    [TestMethod]
    public async Task CreateActivityAsync_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var circleId = 1;
        var createActivityDto = new CreateActivityDto
        {
            Title = "Test Activity",
            Description = "Test Description",
            RewardDescription = "Test Reward",
            RewardPoints = 100,
            ActivityType = ActivityType.Manual,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(1)
        };
        var creatorId = 1;
        var existingCircle = new Circle { CircleId = circleId, Name = "Test Circle", OwnerId = creatorId };

        _mockCircleRepository.Setup(repo => repo.GetByIdAsync(circleId)).ReturnsAsync(existingCircle);
        _mockActivityRepository.Setup(repo => repo.AddAsync(It.IsAny<Activity>())).Returns(Task.CompletedTask);

        // Act
        var result = await _activityService.CreateActivityAsync(circleId, createActivityDto, creatorId);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual("Test Activity", result.Data.Title);
        Assert.AreEqual(100, result.Data.RewardPoints);
        Assert.AreEqual(ActivityType.Manual, result.Data.ActivityType);

        _mockActivityRepository.Verify(repo => repo.AddAsync(It.Is<Activity>(
            a => a.CircleId == circleId &&
                 a.Title == "Test Activity" &&
                 a.RewardPoints == 100 &&
                 a.ActivityType == ActivityType.Manual
        )), Times.Once);
    }

    [TestMethod]
    public async Task GetActivityByIdAsync_ActivityNotFound_ShouldReturnNull()
    {
        // Arrange
        var activityId = 1;
        _mockActivityRepository.Setup(repo => repo.GetByIdAsync(activityId)).ReturnsAsync((Activity)null);

        // Act
        var result = await _activityService.GetActivityByIdAsync(activityId);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetActivityByIdAsync_ValidActivity_ShouldReturnActivityDto()
    {
        // Arrange
        var activityId = 1;
        var activity = new Activity
        {
            ActivityId = activityId,
            CircleId = 1,
            Title = "Test Activity",
            Description = "Test Description",
            RewardDescription = "Test Reward",
            RewardPoints = 100,
            ActivityType = ActivityType.Manual,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(1)
        };

        _mockActivityRepository.Setup(repo => repo.GetByIdAsync(activityId)).ReturnsAsync(activity);
        _mockParticipantRepository.Setup(repo => repo.GetByActivityIdAsync(activityId))
            .ReturnsAsync(new List<ActivityParticipant>()); // 模拟没有参与者

        // Act
        var result = await _activityService.GetActivityByIdAsync(activityId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(activityId, result.ActivityId);
        Assert.AreEqual("Test Activity", result.Title);
        Assert.AreEqual(100, result.RewardPoints);
        Assert.AreEqual(ActivityType.Manual, result.ActivityType);
        Assert.AreEqual(0, result.ParticipantCount); // 验证参与人数
    }
}
