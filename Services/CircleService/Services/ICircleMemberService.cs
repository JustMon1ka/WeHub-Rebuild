using CircleService.DTOs;
using CircleService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CircleService.Services;

/// <summary>
/// 圈子成员服务接口，定义了与圈子成员相关的业务逻辑。
/// </summary>
public interface ICircleMemberService
{
    /// <summary>
    /// 申请加入一个圈子
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    /// <param name="userId">申请人的用户ID</param>
    /// <returns>操作结果的Service层响应</returns>
    Task<ServiceResponse> ApplyToJoinCircleAsync(int circleId, int userId);

    /// <summary>
    /// 获取一个圈子的所有成员
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    /// <param name="sortByPoints">是否按积分降序排序</param>
    /// <returns>返回成员信息DTO的列表</returns>
    Task<IEnumerable<CircleMemberDto>> GetCircleMembersAsync(int circleId, bool sortByPoints = false);
    
    /// <summary>
    /// 审批用户的入圈申请
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    /// <param name="targetUserId">被审批用户的ID</param>
    /// <param name="approverId">审批人（圈主/管理员）的用户ID</param>
    /// <param name="approve">是否通过申请</param>
    /// <param name="role">指定用户角色（可选，默认为普通成员）</param>
    /// <returns>操作结果的Service层响应</returns>
    Task<ServiceResponse> ApproveJoinApplicationAsync(int circleId, int targetUserId, int approverId, bool approve, CircleMemberRole? role = null);

    /// <summary>
    /// 将成员移出圈子
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    /// <param name="targetUserId">被移除用户的ID</param>
    /// <param name="removerId">操作人（圈主/管理员）的用户ID</param>
    /// <returns>操作结果的Service层响应</returns>
    Task<ServiceResponse> RemoveMemberAsync(int circleId, int targetUserId, int removerId);

    /// <summary>
    /// 获取圈子积分排行榜
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    /// <returns>返回按积分排序的成员列表</returns>
    Task<IEnumerable<CircleMemberDto>> GetLeaderboardAsync(int circleId);

    /// <summary>
    /// 获取用户在特定圈子的积分详情
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>返回单个成员的详细信息，如果不存在则返回null</returns>
    Task<CircleMemberDto?> GetMemberDetailsAsync(int circleId, int userId);

    /// <summary>
    /// 用户主动退出一个圈子
    /// </summary>
    /// <param name="circleId">要退出的圈子ID</param>
    /// <param name="userId">执行退出操作的用户ID</param>
    /// <returns>操作结果的Service层响应</returns>
    Task<ServiceResponse> LeaveCircleAsync(int circleId, int userId);

    /// <summary>
    /// 获取圈子的申请列表（待审批和已处理）
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    /// <param name="requesterId">请求者ID（用于权限验证）</param>
    /// <returns>返回申请列表，包含待审批和已处理的申请</returns>
    Task<ApplicationListDto?> GetApplicationsAsync(int circleId, int requesterId);
} 