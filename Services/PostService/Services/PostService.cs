using PostService.Models;
using PostService.Repositories;

namespace PostService.Services
{
    public interface IPostService
    {
        Task<List<Post>> GetPostsByIdsAsync(List<long> ids);
        Task<Post?> GetPostByIdAsync(long postId);
        Task DeletePostAsync(long postId);
        Task<Post> PublishPostAsync(long userId, string content);
    }
    
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;

        public PostService(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        public async Task<List<Post>> GetPostsByIdsAsync(List<long> ids)
        {
            return await _postRepository.GetPostsByIdsAsync(ids);
        }
        
        public async Task<Post?> GetPostByIdAsync(long postId)
        {
            return await _postRepository.GetByIdAsync(postId);
        }

        public async Task DeletePostAsync(long postId)
        {
            await _postRepository.MarkAsDeletedAsync(postId);
        }
        
        public async Task<Post> PublishPostAsync(long userId, string content)
        {
            return await _postRepository.InsertPostAsync(userId, content);
        }
    }
}
