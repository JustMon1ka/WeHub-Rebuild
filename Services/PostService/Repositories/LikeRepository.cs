using Microsoft.EntityFrameworkCore;
using PostService.Data;
using PostService.Models;
using System.Threading.Tasks;

namespace PostService.Repositories
{
    public interface ILikeRepository
    {
        Task IncrementLikeCountAsync(long postId);
        Task DecrementLikeCountAsync(long postId);
        Task<bool> ToggleLikeAsync(int userId, Like like);
    }
    
    public class LikeRepository: ILikeRepository
    {
        private readonly AppDbContext _context;

        public LikeRepository(AppDbContext context)
        {
            _context = context;
        }


        // ✅ 点赞数 +1
        public async Task IncrementLikeCountAsync(long postId)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);
            if (post != null)
            {
                post.Likes = (post.Likes ?? 0) + 1;
                await _context.SaveChangesAsync();
            }
        }

        // ✅ 点赞数 -1
        public async Task DecrementLikeCountAsync(long postId)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);
            if (post != null && (post.Likes ?? 0) > 0)
            {
                post.Likes = post.Likes - 1;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 切换点赞状态
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="like">点赞请求（包含 TargetId、Type、IsLike）</param>
        /// <returns>是否有状态变化（true = 点赞/取消点赞成功，false = 没有变化）</returns>
        public async Task<bool> ToggleLikeAsync(int userId, Like like)
        {
            var existing = await _context.Set<Like>()
                .FirstOrDefaultAsync(l => l.UserId == userId && l.TargetId == like.TargetId && l.TargetType == like.TargetType);

            if (like.IsLike)
            {
                // 点赞
                if (existing == null)
                {
                    like.UserId = userId;
                    _context.Set<Like>().Add(like);
                    await _context.SaveChangesAsync();
                    return true; // 新增成功
                }
                return false; // 已点赞过，没有变化
            }
            else
            {
                // 取消点赞
                if (existing != null)
                {
                    _context.Set<Like>().Remove(existing);
                    await _context.SaveChangesAsync();
                    return true; // 删除成功
                }
                return false; // 原本就没有点赞，不需要改
            }
        }
    }
}
