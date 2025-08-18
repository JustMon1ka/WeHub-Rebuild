using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CircleService.Services;
using CircleService.Repositories;
using CircleService.Models;
using CircleService.DTOs;
using System.Threading.Tasks;

namespace CircleService.Tests;

[TestClass]
public class CircleMemberServiceTests
{
    private Mock<ICircleRepository> _mockCircleRepository;
    private Mock<ICircleMemberRepository> _mockMemberRepository;
    private Mock<IActivityParticipantRepository> _mockActivityParticipantRepository;
    private CircleMemberService _memberService;

    [TestInitialize]
    public void Setup()
    {
        _mockCircleRepository = new Mock<ICircleRepository>();
        _mockMemberRepository = new Mock<ICircleMemberRepository>();
        _mockActivityParticipantRepository = new Mock<IActivityParticipantRepository>();
        _memberService = new CircleMemberService(
            _mockMemberRepository.Object,
            _mockCircleRepository.Object,
            _mockActivityParticipantRepository.Object
        );
    }

    [TestMethod]
    public async Task ApplyToJoinCircleAsync_CircleNotFound_ShouldFail()
    {
        // Arrange
        var circleId = 1;
        var userId = 1;
        _mockCircleRepository.Setup(repo => repo.GetByIdAsync(circleId)).ReturnsAsync((Circle)null);

        // Act
        var result = await _memberService.ApplyToJoinCircleAsync(circleId, userId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("圈子不存在。", result.ErrorMessage);
    }

    [TestMethod]
    public async Task ApplyToJoinCircleAsync_UserAlreadyMember_ShouldFail()
    {
        // Arrange
        var circleId = 1;
        var userId = 1;
        var existingCircle = new Circle { CircleId = circleId, Name = "Test Circle" };
        var existingMember = new CircleMember { CircleId = circleId, UserId = userId, Status = CircleMemberStatus.Approved };

        _mockCircleRepository.Setup(repo => repo.GetByIdAsync(circleId)).ReturnsAsync(existingCircle);
        _mockMemberRepository.Setup(repo => repo.GetByIdAsync(circleId, userId)).ReturnsAsync(existingMember);

        // Act
        var result = await _memberService.ApplyToJoinCircleAsync(circleId, userId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("您已经申请过或已是该圈子成员。", result.ErrorMessage);
    }

    [TestMethod]
    public async Task ApplyToJoinCircleAsync_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var circleId = 1;
        var userId = 2; // Use a different user ID
        var existingCircle = new Circle { CircleId = circleId, Name = "Test Circle" };

        _mockCircleRepository.Setup(repo => repo.GetByIdAsync(circleId)).ReturnsAsync(existingCircle);
        _mockMemberRepository.Setup(repo => repo.GetByIdAsync(circleId, userId)).ReturnsAsync((CircleMember)null); // User is not a member

        // Act
        var result = await _memberService.ApplyToJoinCircleAsync(circleId, userId);

        // Assert
        Assert.IsTrue(result.Success);
        _mockMemberRepository.Verify(repo => repo.AddAsync(It.Is<CircleMember>(
            cm => cm.CircleId == circleId &&
                  cm.UserId == userId &&
                  cm.Status == CircleMemberStatus.Pending
        )), Times.Once);
    }
}
