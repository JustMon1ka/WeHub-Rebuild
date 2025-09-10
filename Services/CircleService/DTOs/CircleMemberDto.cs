using CircleService.Models;

namespace CircleService.DTOs;

/// <summary>
/// 用于传输圈子成员信息的数据传输对象。
/// </summary>
public class CircleMemberDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public int UserId { get; set; }

    // 未来可以扩展，加入用户名、头像等从 UserProfile 获取的信息
    // public string Username { get; set; }
    // public string AvatarUrl { get; set; }

    /// <summary>
    /// 成员在圈子中的角色 (成员/管理员)
    /// </summary>
    public CircleMemberRole Role { get; set; }

    /// <summary>
    /// 成员的积分
    /// </summary>
    public int Points { get; set; }
} 