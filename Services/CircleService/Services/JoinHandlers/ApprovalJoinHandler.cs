using CircleService.Repositories;

namespace CircleService.Services.JoinHandlers;

public class ApprovalJoinHandler : IJoinHandler
{
    private readonly ICircleMemberRepository _memberRepo;

    public ApprovalJoinHandler(ICircleMemberRepository memberRepo)
    {
        _memberRepo = memberRepo;
    }

    public async Task HandleJoinAsync(int circleId, int userId)
    {
        // 提交申请
        await _memberRepo.AddJoinApplicationAsync(circleId, userId);
    }
}
