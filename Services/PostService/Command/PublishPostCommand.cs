using PostService.DTOs;

namespace PostService.Command;

public class PublishPostCommand
{
    public PostPublishRequest Request { get; }
    public long UserId { get; }

    public PublishPostCommand(PostPublishRequest request, long userId)
    {
        Request = request;
        UserId = userId;
    }
}