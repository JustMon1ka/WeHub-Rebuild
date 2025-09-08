using CircleService.Models;

namespace CircleService.DTOs;

/// <summary>
/// 用于传输申请信息的数据传输对象
/// </summary>
public class ApplicationDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 申请时间
    /// </summary>
    public DateTime ApplyTime { get; set; }

    /// <summary>
    /// 申请状态
    /// </summary>
    public CircleMemberStatus Status { get; set; }

    /// <summary>
    /// 处理时间（仅当状态为Approved时有值）
    /// </summary>
    public DateTime? ProcessedTime { get; set; }

    /// <summary>
    /// 分配的角色（仅当状态为Approved时有值）
    /// </summary>
    public CircleMemberRole? Role { get; set; }
}

/// <summary>
/// 用于传输申请列表的数据传输对象
/// </summary>
public class ApplicationListDto
{
    /// <summary>
    /// 待审批的申请列表（按申请时间降序排列）
    /// </summary>
    public IEnumerable<ApplicationDto> PendingApplications { get; set; } = new List<ApplicationDto>();

    /// <summary>
    /// 已处理的申请列表（按处理时间降序排列）
    /// </summary>
    public IEnumerable<ApplicationDto> ProcessedApplications { get; set; } = new List<ApplicationDto>();
}
