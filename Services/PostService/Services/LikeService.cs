using PostService.DTOs;
using PostService.Models;
using PostService.Repositories;

namespace PostService.Services
{
    public class LikeService : ILikeService
    {
        private readonly ILikeRepository _repository;
        private readonly IPostRepository _postRepository;

        public LikeService(ILikeRepository repository, IPostRepository postRepository)
        {
            _repository = repository;
            _postRepository = postRepository;
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
            bool changed = await _repository.ToggleLikeAsync(userId, like);

            // 如果仓库确认数据有变化（即状态确实切换了），再更新 Post.Likes
            if (changed && like.TargetType == "post")
            {
                if (like.IsLike)
                {
                    await _postRepository.IncrementLikeCountAsync(like.TargetId);
                }
                else
                {
                    await _postRepository.DecrementLikeCountAsync(like.TargetId);
                }
            }
        }
    }
}
