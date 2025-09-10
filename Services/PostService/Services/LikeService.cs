using PostService.DTOs;
using PostService.Models;
using PostService.Repositories;

namespace PostService.Services
{  
    public interface ILikeService
    {
        Task ToggleLikeAsync(int userId, LikeRequest request);
    }
    public class LikeService : ILikeService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IPostRepository _postRepository;
        private readonly IPostRedisRepository _redisRepository;

        public LikeService(
            ILikeRepository likeRepository,
            IPostRepository postRepository,
            IPostRedisRepository redisRepository)
        {
            _likeRepository = likeRepository;
            _postRepository = postRepository;
            _redisRepository = redisRepository;
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

            if (changed && like.TargetType == "post")
            {
                var postId = Convert.ToInt64(like.TargetId);

                if (like.IsLike)
                {
                    await _likeRepository.IncrementLikeCountAsync(postId);

                    // ✅ 查帖子找到作者
                    var post = await _postRepository.GetByIdAsync(postId);
                    if (post != null && post.UserId.HasValue)
                    {
                        // ✅ 调用 PostRedisRepository 插入一条未读通知
                        await _redisRepository.InsertUnreadNoticeAsync(post.UserId, "like", postId);
                    }
                }
                else
                {
                    await _likeRepository.DecrementLikeCountAsync(postId);
                }
            }
        }
    }
}
