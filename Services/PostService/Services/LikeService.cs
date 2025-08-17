using PostService.DTOs;
using PostService.Models;
using PostService.Repositories;

namespace PostService.Services
{
    public class LikeService
    {
        private readonly LikeRepository _repository;

        public LikeService(LikeRepository repository)
        {
            _repository = repository;
        }

        public async Task ToggleLikeAsync(int userId, LikeRequest request)
        {
            var like = new Like
            {
                TargetId = request.TargetId,
                TargetType = request.Type,
                IsLike = request.Like
            };
            await _repository.ToggleLikeAsync(userId, like);
        }
    }
}
