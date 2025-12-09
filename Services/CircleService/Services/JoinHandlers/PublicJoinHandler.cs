using CircleService.Repositories;
using CircleService.Models;

namespace CircleService.Services.JoinHandlers;

public class PublicJoinHandler : IJoinHandler
{
    private readonly ICircleMemberRepository _memberRepo;

    public PublicJoinHandler(ICircleMemberRepository memberRepo)
    {
        _memberRepo = memberRepo;
    }

    public async Task HandleJoinAsync(int circleId, int userId)
    {
        // 用户直接加入圈子
        await _memberRepo.AddAsync(new CircleMember
        {
            CircleId = circleId,
            UserId = userId,
            Role = CircleMemberRole.Member
        });
    }
}
