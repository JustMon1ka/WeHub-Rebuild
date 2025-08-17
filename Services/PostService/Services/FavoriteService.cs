using PostService.DTOs;
using PostService.Repositories;

namespace PostService.Services
{
    public class FavoriteService
    {
        private readonly FavoriteRepository _repository;

        public FavoriteService(FavoriteRepository repository)
        {
            _repository = repository;
        }

        public async Task ToggleFavoriteAsync(int userId, int postId)
        {
            await _repository.ToggleFavoriteAsync(userId, postId);
        }

        public async Task<List<int>> GetFavoritesAsync(int userId)
        {
            var favorites = await _repository.GetFavoritesAsync(userId);
            return favorites.Select(f => f.PostId).ToList();
        }
    }
}
