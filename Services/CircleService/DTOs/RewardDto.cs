namespace CircleService.DTOs;

/// <summary>
/// 用于传输活动奖励信息的数据传输对象。
/// </summary>
public class RewardDto
{
    /// <summary>
    /// 奖励的文字描述
    /// </summary>
    public string RewardDescription { get; set; }

    /// <summary>
    /// 本次奖励获得的积分
    /// </summary>
    public int PointsAwarded { get; set; }
} 