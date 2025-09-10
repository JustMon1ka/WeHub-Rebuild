using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CircleService.Services;
using CircleService.Repositories;
using CircleService.Models;
using CircleService.DTOs;
using System.Threading.Tasks;

namespace CircleService.Tests;

[TestClass]
public class ActivityParticipantServiceTests
{
    private Mock<IActivityParticipantRepository> _mockParticipantRepository;
    private Mock<IActivityRepository> _mockActivityRepository;
    private Mock<ICircleMemberRepository> _mockMemberRepository;
    private ActivityParticipantService _participantService;

    [TestInitialize]
    public void Setup()
    {
        _mockParticipantRepository = new Mock<IActivityParticipantRepository>();
        _mockActivityRepository = new Mock<IActivityRepository>();
        _mockMemberRepository = new Mock<ICircleMemberRepository>();
        _participantService = new ActivityParticipantService(
            _mockParticipantRepository.Object,
            _mockActivityRepository.Object,
            _mockMemberRepository.Object
        );
    }

    [TestMethod]
    public async Task JoinActivityAsync_ActivityNotFound_ShouldFail()
    {
        // Arrange
        var activityId = 1;
        var userId = 1;
        _mockActivityRepository.Setup(repo => repo.GetByIdAsync(activityId)).ReturnsAsync((Activity)null);

        // Act
        var result = await _participantService.JoinActivityAsync(activityId, userId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("活动不存在或已结束。", result.ErrorMessage);
    }

    [TestMethod]
    public async Task JoinActivityAsync_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var activityId = 1;
        var userId = 1;
        var activeActivity = new Activity
        {
            ActivityId = activityId,
            Title = "Active Activity",
            EndTime = DateTime.UtcNow.AddDays(1) // 未过期
        };

        _mockActivityRepository.Setup(repo => repo.GetByIdAsync(activityId)).ReturnsAsync(activeActivity);
        _mockParticipantRepository.Setup(repo => repo.GetByIdAsync(activityId, userId)).ReturnsAsync((ActivityParticipant)null);
        _mockParticipantRepository.Setup(repo => repo.AddAsync(It.IsAny<ActivityParticipant>())).Returns(Task.CompletedTask);

        // Act
        var result = await _participantService.JoinActivityAsync(activityId, userId);

        // Assert
        Assert.IsTrue(result.Success);

        _mockParticipantRepository.Verify(repo => repo.AddAsync(It.Is<ActivityParticipant>(
            p => p.ActivityId == activityId &&
                 p.UserId == userId &&
                 p.Status == ParticipantStatus.InProgress
        )), Times.Once);
    }

    [TestMethod]
    public async Task GetUserActivityParticipationsAsync_ValidUserId_ShouldReturnParticipations()
    {
        // Arrange
        var userId = 1;
        var participations = new List<ActivityParticipant>
        {
            new ActivityParticipant
            {
                ActivityId = 1,
                UserId = userId,
                Status = ParticipantStatus.Completed,
                RewardStatus = RewardStatus.Claimed,
                JoinTime = DateTime.UtcNow.AddDays(-1)
            }
        };

        var activity = new Activity
        {
            ActivityId = 1,
            CircleId = 1,
            Title = "Activity 1",
            RewardPoints = 100,
            ActivityType = ActivityType.Manual,
            StartTime = DateTime.UtcNow.AddDays(-2),
            EndTime = DateTime.UtcNow.AddDays(1)
        };

        _mockParticipantRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(participations);
        _mockActivityRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(activity);

        // Act
        var result = await _participantService.GetUserActivityParticipationsAsync(userId);

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);

        var firstParticipation = resultList.First();
        Assert.AreEqual("Activity 1", firstParticipation.ActivityTitle);
        Assert.AreEqual(ParticipantStatus.Completed, firstParticipation.Status);
        Assert.AreEqual(100, firstParticipation.RewardPoints);
    }

    [TestMethod]
    public async Task GetUserActivityParticipationAsync_UserNotParticipated_ShouldReturnNull()
    {
        // Arrange
        var activityId = 1;
        var userId = 1;
        _mockParticipantRepository.Setup(repo => repo.GetByIdAsync(activityId, userId)).ReturnsAsync((ActivityParticipant)null);

        // Act
        var result = await _participantService.GetUserActivityParticipationAsync(activityId, userId);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task ClaimRewardAsync_UserNotParticipated_ShouldFail()
    {
        // Arrange
        var activityId = 1;
        var userId = 1;
        _mockParticipantRepository.Setup(repo => repo.GetByIdAsync(activityId, userId)).ReturnsAsync((ActivityParticipant)null);

        // Act
        var result = await _participantService.ClaimRewardAsync(activityId, userId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("您尚未完成此活动，无法领取奖励。", result.ErrorMessage);
    }

    [TestMethod]
    public async Task ClaimRewardAsync_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var activityId = 1;
        var userId = 1;
        var participation = new ActivityParticipant
        {
            ActivityId = activityId,
            UserId = userId,
            Status = ParticipantStatus.Completed,
            RewardStatus = RewardStatus.NotClaimed
        };

        var activity = new Activity
        {
            ActivityId = activityId,
            Title = "Test Activity",
            RewardDescription = "Test Reward",
            RewardPoints = 100
        };

        _mockParticipantRepository.Setup(repo => repo.GetByIdAsync(activityId, userId)).ReturnsAsync(participation);
        _mockActivityRepository.Setup(repo => repo.GetByIdAsync(activityId)).ReturnsAsync(activity);
        _mockParticipantRepository.Setup(repo => repo.UpdateAsync(It.IsAny<ActivityParticipant>())).Returns(Task.CompletedTask);

        // Act
        var result = await _participantService.ClaimRewardAsync(activityId, userId);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual("Test Reward", result.Data.RewardDescription);
        Assert.AreEqual(100, result.Data.PointsAwarded);

        _mockParticipantRepository.Verify(repo => repo.UpdateAsync(It.Is<ActivityParticipant>(
            p => p.ActivityId == activityId &&
                 p.UserId == userId &&
                 p.RewardStatus == RewardStatus.Claimed
        )), Times.Once);
    }
}
