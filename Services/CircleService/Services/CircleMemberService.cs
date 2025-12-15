using CircleService.DTOs;
using CircleService.Models;
using CircleService.Repositories;
using CircleService.Services.JoinHandlers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CircleService.Services;

/// <summary>
/// 圈子成员服务的实现类，负责处理圈子成员相关的业务逻辑。
/// </summary>
public class CircleMemberService : ICircleMemberService
{
    private readonly ICircleMemberRepository _memberRepository;
    private readonly ICircleRepository _circleRepository;
    private readonly IActivityParticipantRepository _activityParticipantRepository;
    private readonly JoinHandlerFactory _joinHandlerFactory;

    public CircleMemberService(
        ICircleMemberRepository memberRepository, 
        ICircleRepository circleRepository,
        IActivityParticipantRepository activityParticipantRepository)
    {
        _memberRepository = memberRepository;
        _circleRepository = circleRepository;
        _activityParticipantRepository = activityParticipantRepository;
    }

    public async Task<ServiceResponse> ApplyToJoinCircleAsync(int circleId, int userId)
    {
        var circle = await _circleRepository.GetByIdAsync(circleId);
        if (circle == null)
        {
            return ServiceResponse.Fail("圈子不存在。");
        }

        var existingMember = await _memberRepository.GetByIdAsync(circleId, userId);
        if (existingMember != null)
        {
            return ServiceResponse.Fail("您已是该圈子成员或已提交申请。");
        }

        var newMember = new CircleMember
        {
            CircleId = circleId,
            UserId = userId,
            Role = CircleMemberRole.Member,
            Status = CircleMemberStatus.Pending // 默认为待审核
        };

        await _memberRepository.AddAsync(newMember);
        return ServiceResponse.Succeed();
    }

    public async Task<IEnumerable<CircleMemberDto>> GetCircleMembersAsync(int circleId, bool sortByPoints = false)
    {
        var members = await _memberRepository.GetMembersByCircleIdAsync(circleId);
        
        // 只返回已批准的成员
        var approvedMembers = members.Where(m => m.Status == CircleMemberStatus.Approved);
        
        // 根据参数决定是否按积分排序
        if (sortByPoints)
        {
            approvedMembers = approvedMembers.OrderByDescending(m => m.Points);
        }
        
        return approvedMembers.Select(MapToCircleMemberDto);
    }
    
    public async Task<ServiceResponse> ApproveJoinApplicationAsync(int circleId, int targetUserId, int approverId, bool approve, CircleMemberRole? role = null)
    {
        var circle = await _circleRepository.GetByIdAsync(circleId);
        if (circle == null || circle.OwnerId != approverId)
        {
            // 简化权限判断：只有圈主可以审批
            return ServiceResponse.Fail("权限不足。");
        }

        var member = await _memberRepository.GetByIdAsync(circleId, targetUserId);
        if (member == null || member.Status != CircleMemberStatus.Pending)
        {
            return ServiceResponse.Fail("该用户没有待审批的申请。");
        }

        if (approve)
        {
            member.Status = CircleMemberStatus.Approved;
            member.ProcessedTime = DateTime.UtcNow; // 设置处理时间
            // 如果指定了角色，则使用指定角色；否则保持默认角色（普通成员）
            if (role.HasValue)
            {
                member.Role = role.Value;
            }
            await _memberRepository.UpdateAsync(member);
        }
        else
        {
            // 拒绝则直接删除申请记录
            await _memberRepository.RemoveAsync(member);
        }

        return ServiceResponse.Succeed();
    }
    
    public async Task<ServiceResponse> RemoveMemberAsync(int circleId, int targetUserId, int removerId)
    {
        var circle = await _circleRepository.GetByIdAsync(circleId);
        if (circle == null || circle.OwnerId != removerId)
        {
            // 简化权限判断：只有圈主可以移除成员
            return ServiceResponse.Fail("权限不足。");
        }

        if (targetUserId == removerId)
        {
            return ServiceResponse.Fail("圈主不能移除自己。");
        }
        
        var member = await _memberRepository.GetByIdAsync(circleId, targetUserId);
        if (member == null)
        {
            return ServiceResponse.Fail("该用户不是圈子成员。");
        }

        await _memberRepository.RemoveAsync(member);
        return ServiceResponse.Succeed();
    }

    public async Task<IEnumerable<CircleMemberDto>> GetLeaderboardAsync(int circleId)
    {
        var members = await _memberRepository.GetMembersByCircleIdAsync(circleId);
        
        // 返回按积分降序排列的已批准成员
        return members.Where(m => m.Status == CircleMemberStatus.Approved)
                      .OrderByDescending(m => m.Points)
                      .Select(MapToCircleMemberDto);
    }

    public async Task<CircleMemberDto?> GetMemberDetailsAsync(int circleId, int userId)
    {
        var member = await _memberRepository.GetByIdAsync(circleId, userId);
        if (member == null || member.Status != CircleMemberStatus.Approved)
        {
            return null;
        }

        return MapToCircleMemberDto(member);
    }

    public async Task<ServiceResponse> LeaveCircleAsync(int circleId, int userId)
    {
        var circle = await _circleRepository.GetByIdAsync(circleId);
        if (circle == null)
        {
            return ServiceResponse.Fail("圈子不存在。");
        }

        if (circle.OwnerId == userId)
        {
            return ServiceResponse.Fail("圈主不能退出自己的圈子。");
        }

        var member = await _memberRepository.GetByIdAsync(circleId, userId);
        if (member == null)
        {
            return ServiceResponse.Fail("您不是该圈子的成员。");
        }

        // 1. 先删除该用户在此圈子内的所有活动参与记录
        await _activityParticipantRepository.RemoveUserParticipationInCircleAsync(circleId, userId);

        // 2. 再移除该用户的圈子成员身份
        await _memberRepository.RemoveAsync(member);
        
        return ServiceResponse.Succeed();
    }

    private CircleMemberDto MapToCircleMemberDto(CircleMember member)
    {
        return new CircleMemberDto
        {
            UserId = member.UserId,
            Role = member.Role,
            Points = member.Points
            // 未来可以从 UserDataService 获取更多用户信息并填充
        };
    }

    public async Task<ApplicationListDto?> GetApplicationsAsync(int circleId, int requesterId)
    {
        // 验证请求者是否为圈主或管理员
        var circle = await _circleRepository.GetByIdAsync(circleId);
        if (circle == null)
        {
            return null;
        }

        // 检查权限：圈主或管理员可以查看申请列表
        if (circle.OwnerId != requesterId)
        {
            // 如果不是圈主，检查是否为管理员
            var member = await _memberRepository.GetByIdAsync(circleId, requesterId);
            if (member == null || member.Status != CircleMemberStatus.Approved || member.Role != CircleMemberRole.Admin)
            {
                return null;
            }
        }

        // 获取所有申请记录
        var allApplications = await _memberRepository.GetAllApplicationsByCircleIdAsync(circleId);

        // 分离待审批和已处理的申请
        var pendingApplications = allApplications
            .Where(app => app.Status == CircleMemberStatus.Pending)
            .OrderByDescending(app => app.ApplyTime)
            .Select(MapToApplicationDto);

        var processedApplications = allApplications
            .Where(app => app.Status == CircleMemberStatus.Approved)
            .OrderByDescending(app => app.ProcessedTime ?? app.ApplyTime)
            .Select(MapToApplicationDto);

        return new ApplicationListDto
        {
            PendingApplications = pendingApplications,
            ProcessedApplications = processedApplications
        };
    }
    //Refactored
    private ApplicationDto MapToApplicationDto(CircleMember member)
    {
        return new ApplicationDto
        {
            UserId = member.UserId,
            ApplyTime = member.ApplyTime,
            Status = member.Status,
            ProcessedTime = member.ProcessedTime,
            Role = member.Status == CircleMemberStatus.Approved ? member.Role : null
        };
    }

    public async Task JoinCircleAsync(int circleId, int userId)
{
    var circle = await _circleRepository.GetByIdAsync(circleId);
    if (circle == null)
        throw new Exception("Circle not found.");

    var handler = _joinHandlerFactory.Create(circle.JoinType);

    await handler.HandleJoinAsync(circleId, userId);
}
} 
