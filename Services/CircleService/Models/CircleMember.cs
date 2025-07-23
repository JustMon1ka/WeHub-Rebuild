using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CircleService.Models;

/// <summary>
/// 成员角色 (普通成员/管理员)
/// </summary>
public enum CircleMemberRole
{
    Member,
    Admin
}

/// <summary>
/// 成员状态 (待审核/已批准)
/// </summary>
public enum CircleMemberStatus
{
    Pending,
    Approved
}

/// <summary>
/// 圈子成员表
/// </summary>
[Table("CIRCLE_MEMBERS")]
public class CircleMember
{
    /// <summary>
    /// 圈子ID (复合主键)
    /// </summary>
    [Key]
    [Column("CIRCLE_ID", Order = 0)]
    public int CircleId { get; set; }

    /// <summary>
    /// 用户ID (复合主键)
    /// </summary>
    [Key]
    [Column("USER_ID", Order = 1)]
    public int UserId { get; set; }

    /// <summary>
    /// 成员在圈子中的角色
    /// </summary>
    [Column("ROLE")]
    public CircleMemberRole Role { get; set; } = CircleMemberRole.Member;
    
    /// <summary>
    /// 成员加入圈子的状态
    /// </summary>
    [Column("STATUS")]
    public CircleMemberStatus Status { get; set; } = CircleMemberStatus.Pending;

    /// <summary>
    /// 成员在圈子内的积分
    /// </summary>
    [Column("POINTS")]
    public int Points { get; set; } = 0;
} 