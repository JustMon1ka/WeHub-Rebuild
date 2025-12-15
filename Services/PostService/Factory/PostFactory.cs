// refactoring begin
using Models;

namespace PostService.Factory;

public static class PostFactory
{
    public static Post Create(long userId, long circleId, string title, string content)
    {
        return new Post
        {
            UserId = userId,
            CircleId = circleId,
            Title = title,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = 0,
            IsHidden = 0,
            Views = 0,
            Likes = 0,
            Dislikes = 0
        };
    }
}
// refactoring end