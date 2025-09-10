using Microsoft.VisualStudio.TestTools.UnitTesting;
using CircleService.DTOs;
using CircleService.Models;

namespace CircleService.Tests;

[TestClass]
public class DtoDefaultValuesTests
{
    [TestMethod]
    public void CreateActivityDto_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var dto = new CreateActivityDto
        {
            Title = "Test Activity"
        };

        // Assert
        Assert.AreEqual(100, dto.RewardPoints, "默认奖励点数应为100");
        Assert.AreEqual(ActivityType.Manual, dto.ActivityType, "默认活动类型应为手动完成");
        
        // 检查时间默认值（允许1秒的误差）
        var now = DateTime.UtcNow;
        var oneMonthLater = now.AddMonths(1);
        
        Assert.IsTrue(Math.Abs((dto.StartTime - now).TotalSeconds) < 1, "开始时间应默认为当前时间");
        Assert.IsTrue(Math.Abs((dto.EndTime - oneMonthLater).TotalSeconds) < 1, "结束时间应默认为一个月后");
    }

    [TestMethod]
    public void UpdateActivityDto_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var dto = new UpdateActivityDto
        {
            Title = "Test Activity"
        };

        // Assert
        Assert.AreEqual(100, dto.RewardPoints, "默认奖励点数应为100");
        Assert.AreEqual(ActivityType.Manual, dto.ActivityType, "默认活动类型应为手动完成");
        
        // 检查时间默认值（允许1秒的误差）
        var now = DateTime.UtcNow;
        var oneMonthLater = now.AddMonths(1);
        
        Assert.IsTrue(Math.Abs((dto.StartTime - now).TotalSeconds) < 1, "开始时间应默认为当前时间");
        Assert.IsTrue(Math.Abs((dto.EndTime - oneMonthLater).TotalSeconds) < 1, "结束时间应默认为一个月后");
    }

    [TestMethod]
    public void CreateActivityDto_ExplicitValues_ShouldOverrideDefaults()
    {
        // Arrange
        var customStartTime = DateTime.UtcNow.AddDays(1);
        var customEndTime = DateTime.UtcNow.AddDays(7);
        var customRewardPoints = 500;
        var customActivityType = ActivityType.PostCreation;

        // Act
        var dto = new CreateActivityDto
        {
            Title = "Test Activity",
            StartTime = customStartTime,
            EndTime = customEndTime,
            RewardPoints = customRewardPoints,
            ActivityType = customActivityType
        };

        // Assert
        Assert.AreEqual(customStartTime, dto.StartTime);
        Assert.AreEqual(customEndTime, dto.EndTime);
        Assert.AreEqual(customRewardPoints, dto.RewardPoints);
        Assert.AreEqual(customActivityType, dto.ActivityType);
    }
}
