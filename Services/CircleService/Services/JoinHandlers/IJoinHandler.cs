namespace CircleService.Services.JoinHandlers;

public interface IJoinHandler
{
    Task HandleJoinAsync(int circleId, int userId);
}
