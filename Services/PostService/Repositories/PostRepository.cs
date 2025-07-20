using Microsoft.EntityFrameworkCore;
using PostService.Data;
using PostService.Models;

namespace PostService.Repositories
{
    public interface IPostRepository
    {
        Task<List<Post>> GetPostsByIdsAsync(List<long> ids);
        Task<Post?> GetByIdAsync(long postId);
        Task MarkAsDeletedAsync(long postId);
        Task<Post> InsertPostAsync(long userId, string content);
    }
    
    public class PostRepository : IPostRepository
    {
        private readonly AppDbContext _context;

        public PostRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Post>> GetPostsByIdsAsync(List<long> ids)
        {
            return await _context.Posts
                .Where(p => ids.Contains(p.PostId) && p.IsDeleted == 0)
                .ToListAsync();
        }
        
        public async Task<Post?> GetByIdAsync(long postId)
        {
            return await _context.Posts.FindAsync(postId);
        }

        public async Task MarkAsDeletedAsync(long postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post != null)
            {
                post.IsDeleted = 1;
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
            }
        }
        
        public async Task<Post> InsertPostAsync(long userId, string content)
        {
            var post = new Post
            {
                UserId = userId,
                Content = content,
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            await _context.Entry(post).ReloadAsync();

            return post;
        }
    }
}