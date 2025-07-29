using Microsoft.EntityFrameworkCore;
using Models;
using PostService.Data;
using PostService.Models;

namespace PostService.Repositories
{
    public interface IPostRepository
    {
        Task<List<Post>> GetPostsByIdsAsync(List<long> ids);
        Task<Post?> GetByIdAsync(long postId);
        Task MarkAsDeletedAsync(long postId);
        Task<Post> InsertPostAsync(long userId, long circleId, string title, string content, List<long> tags);
        Task<List<string>> GetTagNamesByPostIdAsync(long postId);
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
            var posts = await _context.Posts
                .Where(p => ids.Contains(p.PostId) && p.IsDeleted == 0)
                .ToListAsync();

            if (!posts.Any()) return posts;

            // 一次性查询所有 PostTag + TagName
            var postTags = await (from pt in _context.PostTags
                join t in _context.Tags on pt.TagId equals t.TagId
                where ids.Contains(pt.PostId)
                select new
                {
                    pt.PostId,
                    t.TagName
                }).ToListAsync();

            // 用 Dictionary 分组映射
            var tagLookup = postTags
                .GroupBy(x => x.PostId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.TagName).ToList());

            // 附加到 Post 对象
            foreach (var post in posts)
            {
                post.TagNames = tagLookup.TryGetValue(post.PostId, out var tags)
                    ? tags
                    : new List<string>();
            }

            return posts;
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
        
        public async Task<Post> InsertPostAsync(long userId, long circleId, string title, string content, List<long> tags)
        {
            var post = new Post
            {
                UserId = userId,
                CircleId = (int)circleId,
                Title = title,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = 0,
                IsHidden = 0,
                Views = 0,
                Likes = 0,
                Dislikes = 0
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();  // 先保存 Post，生成 PostId

            // 处理 Tags → POSTTAG
            if (tags != null && tags.Count > 0)
            {
                foreach (var tagId in tags)
                {
                    var postTag = new PostTag
                    {
                        PostId = post.PostId,
                        TagId = tagId
                    };
                    _context.Set<PostTag>().Add(postTag);
                }
                await _context.SaveChangesAsync();
            }

            await _context.Entry(post).ReloadAsync();
            return post;
        }
        
        public async Task<List<string>> GetTagNamesByPostIdAsync(long postId)
        {
            return await _context.PostTags
                .Where(pt => pt.PostId == postId)
                .Include(pt => pt.Tag)
                .Select(pt => pt.Tag.TagName)
                .ToListAsync();
        }
    }
}