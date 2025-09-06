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
        Assert.AreEqual("您已是该圈子成员或已提交申请。", result.ErrorMessage);
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

    [TestMethod]
    public async Task ApproveJoinApplicationAsync_NotOwner_ShouldFail()
    {
        // Arrange
        var circleId = 1;
        var targetUserId = 2;
        var approverId = 3; // 不是圈主
        var approve = true;

        var circle = new Circle { CircleId = circleId, Name = "Test Circle", OwnerId = 1 }; // 圈主是用户1
        var member = new CircleMember 
        { 
            CircleId = circleId, 
            UserId = targetUserId, 
            Status = CircleMemberStatus.Pending 
        };

        _mockCircleRepository.Setup(repo => repo.GetByIdAsync(circleId)).ReturnsAsync(circle);
        _mockMemberRepository.Setup(repo => repo.GetByIdAsync(circleId, targetUserId)).ReturnsAsync(member);

        // Act
        var result = await _memberService.ApproveJoinApplicationAsync(circleId, targetUserId, approverId, approve);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("权限不足。", result.ErrorMessage);
    }

    [TestMethod]
    public async Task ApproveJoinApplicationAsync_NoPendingApplication_ShouldFail()
    {
        // Arrange
        var circleId = 1;
        var targetUserId = 2;
        var approverId = 1; // 圈主
        var approve = true;

        var circle = new Circle { CircleId = circleId, Name = "Test Circle", OwnerId = approverId };
        var member = new CircleMember 
        { 
            CircleId = circleId, 
            UserId = targetUserId, 
            Status = CircleMemberStatus.Approved // 已经是已批准状态
        };

        _mockCircleRepository.Setup(repo => repo.GetByIdAsync(circleId)).ReturnsAsync(circle);
        _mockMemberRepository.Setup(repo => repo.GetByIdAsync(circleId, targetUserId)).ReturnsAsync(member);

        // Act
        var result = await _memberService.ApproveJoinApplicationAsync(circleId, targetUserId, approverId, approve);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("该用户没有待审批的申请。", result.ErrorMessage);
    }

    [TestMethod]
    public async Task ApproveJoinApplicationAsync_ApproveWithDefaultRole_ShouldSucceed()
    {
        // Arrange
        var circleId = 1;
        var targetUserId = 2;
        var approverId = 1; // 圈主
        var approve = true;
        CircleMemberRole? role = null; // 使用默认角色

        var circle = new Circle { CircleId = circleId, Name = "Test Circle", OwnerId = approverId };
        var member = new CircleMember 
        { 
            CircleId = circleId, 
            UserId = targetUserId, 
            Status = CircleMemberStatus.Pending,
            Role = CircleMemberRole.Member // 默认角色
        };

        _mockCircleRepository.Setup(repo => repo.GetByIdAsync(circleId)).ReturnsAsync(circle);
        _mockMemberRepository.Setup(repo => repo.GetByIdAsync(circleId, targetUserId)).ReturnsAsync(member);
        _mockMemberRepository.Setup(repo => repo.UpdateAsync(It.IsAny<CircleMember>())).Returns(Task.CompletedTask);

        // Act
        var result = await _memberService.ApproveJoinApplicationAsync(circleId, targetUserId, approverId, approve, role);

        // Assert
        Assert.IsTrue(result.Success);

        _mockMemberRepository.Verify(repo => repo.UpdateAsync(It.Is<CircleMember>(
            cm => cm.CircleId == circleId &&
                  cm.UserId == targetUserId &&
                  cm.Status == CircleMemberStatus.Approved &&
                  cm.Role == CircleMemberRole.Member // 保持默认角色
        )), Times.Once);
    }

    [TestMethod]
    public async Task ApproveJoinApplicationAsync_ApproveWithAdminRole_ShouldSucceed()
    {
        // Arrange
        var circleId = 1;
        var targetUserId = 2;
        var approverId = 1; // 圈主
        var approve = true;
        CircleMemberRole? role = CircleMemberRole.Admin; // 指定为管理员

        var circle = new Circle { CircleId = circleId, Name = "Test Circle", OwnerId = approverId };
        var member = new CircleMember 
        { 
            CircleId = circleId, 
            UserId = targetUserId, 
            Status = CircleMemberStatus.Pending,
            Role = CircleMemberRole.Member
        };

        _mockCircleRepository.Setup(repo => repo.GetByIdAsync(circleId)).ReturnsAsync(circle);
        _mockMemberRepository.Setup(repo => repo.GetByIdAsync(circleId, targetUserId)).ReturnsAsync(member);
        _mockMemberRepository.Setup(repo => repo.UpdateAsync(It.IsAny<CircleMember>())).Returns(Task.CompletedTask);

        // Act
        var result = await _memberService.ApproveJoinApplicationAsync(circleId, targetUserId, approverId, approve, role);

        // Assert
        Assert.IsTrue(result.Success);

        _mockMemberRepository.Verify(repo => repo.UpdateAsync(It.Is<CircleMember>(
            cm => cm.CircleId == circleId &&
                  cm.UserId == targetUserId &&
                  cm.Status == CircleMemberStatus.Approved &&
                  cm.Role == CircleMemberRole.Admin // 设置为管理员角色
        )), Times.Once);
    }

    [TestMethod]
    public async Task ApproveJoinApplicationAsync_RejectApplication_ShouldRemoveMember()
    {
        // Arrange
        var circleId = 1;
        var targetUserId = 2;
        var approverId = 1; // 圈主
        var approve = false; // 拒绝申请

        var circle = new Circle { CircleId = circleId, Name = "Test Circle", OwnerId = approverId };
        var member = new CircleMember 
        { 
            CircleId = circleId, 
            UserId = targetUserId, 
            Status = CircleMemberStatus.Pending
        };

        _mockCircleRepository.Setup(repo => repo.GetByIdAsync(circleId)).ReturnsAsync(circle);
        _mockMemberRepository.Setup(repo => repo.GetByIdAsync(circleId, targetUserId)).ReturnsAsync(member);
        _mockMemberRepository.Setup(repo => repo.RemoveAsync(It.IsAny<CircleMember>())).Returns(Task.CompletedTask);

        // Act
        var result = await _memberService.ApproveJoinApplicationAsync(circleId, targetUserId, approverId, approve);

        // Assert
        Assert.IsTrue(result.Success);

        _mockMemberRepository.Verify(repo => repo.RemoveAsync(It.Is<CircleMember>(
            cm => cm.CircleId == circleId &&
                  cm.UserId == targetUserId
        )), Times.Once);
    }
}
