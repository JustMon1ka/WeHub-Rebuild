using PostService.DTOs;
using PostService.Models;
using PostService.Repositories;
using StackExchange.Redis;  // 引入 Redis

namespace PostService.Services
{
    public class LikeService : ILikeService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IPostRepository _postRepository;   // 需要获取被点赞帖子的作者
        private readonly IConnectionMultiplexer _redis;     // Redis 连接

        public LikeService(
            ILikeRepository likeRepository,
            IPostRepository postRepository,
            IConnectionMultiplexer redis)
        {
            _likeRepository = likeRepository;
            _postRepository = postRepository;
            _redis = redis;
        }

        public async Task ToggleLikeAsync(int userId, LikeRequest request)
        {
            var like = new Like
            {
                TargetId = request.TargetId,
                TargetType = request.Type,
                IsLike = request.Like
            };

            // 调用仓库处理点赞/取消点赞
            bool changed = await _likeRepository.ToggleLikeAsync(userId, like);

            // 如果仓库确认数据有变化（即状态确实切换了），再更新 Post.Likes
            if (changed && like.TargetType == "post")
            {
                if (like.IsLike)
                {
                    await _likeRepository.IncrementLikeCountAsync(like.TargetId);

                    // 取到被点赞的帖子，找到作者
                    var post = await _postRepository.GetByIdAsync(like.TargetId);
                    if (post != null && post.UserId.HasValue)
                    {
                        var db = _redis.GetDatabase();
                        string message = $"like:{like.TargetId}";
                        await db.StringSetAsync($"notify:{post.UserId.Value}", message);
                    }
                }
                else
                {
                    await _likeRepository.DecrementLikeCountAsync(like.TargetId);
                }
            }
        }
    }

    public interface ILikeService
    {
        Task ToggleLikeAsync(int userId, LikeRequest request);
    }
}
