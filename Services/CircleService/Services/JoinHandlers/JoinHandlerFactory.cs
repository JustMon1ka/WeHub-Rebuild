using CircleService.Repositories;

namespace CircleService.Services.JoinHandlers;

public class JoinHandlerFactory
{
    private readonly ICircleMemberRepository _memberRepo;

    public JoinHandlerFactory(ICircleMemberRepository memberRepo)
    {
        _memberRepo = memberRepo;
    }

    public IJoinHandler Create(string joinType)
    {
        return joinType switch
        {
            "open" => new PublicJoinHandler(_memberRepo),
            "approval" => new ApprovalJoinHandler(_memberRepo),
            _ => throw new Exception("Unsupported join type: " + joinType)
        };
    }
}
