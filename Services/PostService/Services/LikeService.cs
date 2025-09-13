using PostService.DTOs;
using PostService.Models;
using PostService.Repositories;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace PostService.Services
{
    public interface ILikeService
    {
        Task ToggleLikeAsync(long userId, LikeRequest request);
        Task<bool> GetLikeStatusAsync(long userId, string targetType, long targetId);
    }
    public class LikeService : ILikeService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IPostRepository _postRepository;
        private readonly IPostRedisRepository _redisRepository;
        private readonly HttpClient _httpClient;

        public LikeService(
            ILikeRepository likeRepository,
            IPostRepository postRepository,
            IPostRedisRepository redisRepository,
            HttpClient httpClient)
        {
            _likeRepository = likeRepository;
            _postRepository = postRepository;
            _redisRepository = redisRepository;
            _httpClient = httpClient;
        }

        public async Task ToggleLikeAsync(long userId, LikeRequest request)
        {
            var like = new Like
            {
                UserId = userId,
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

                        // ✅ 调用 NoticeService 创建点赞通知
                        await CreateLikeNotificationAsync((int)userId, (int)post.UserId, (int)postId, "POST");
                    }
                }
                else
                {
                    await _likeRepository.DecrementLikeCountAsync(postId);
                }
            }
            else if (changed && like.TargetType == "post")
            {
                var commentId = Convert.ToInt64(like.TargetId);
                if (like.IsLike)
                {
                    await _likeRepository.IncrementLikeCommentCountAsync(commentId);
                }
                else
                {
                    await _likeRepository.DecrementLikeCommentCountAsync(commentId);
                }
            }
        }

        public async Task<bool> GetLikeStatusAsync(long userId, string targetType, long targetId)
        {
            return await _likeRepository.GetLikeStatusAsync(userId, targetType, targetId);
        }

        private async Task CreateLikeNotificationAsync(int likerId, int targetUserId, int targetId, string targetType)
        {
            try
            {
                // 如果点赞者就是内容作者，不需要创建通知
                if (likerId == targetUserId)
                {
                    Console.WriteLine($"[LikeService] 用户给自己的内容点赞，跳过通知创建");
                    return;
                }

                var request = new
                {
                    likerId = likerId,
                    targetUserId = targetUserId,
                    targetId = targetId,
                    targetType = targetType,
                    likeType = 1
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("http://localhost:5000/api/notifications/likes", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[LikeService] 点赞通知创建成功: LikerId={likerId}, TargetUserId={targetUserId}, TargetId={targetId}, TargetType={targetType}");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[LikeService] 点赞通知创建失败: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LikeService] 创建点赞通知时发生异常: {ex.Message}");
                // 不抛出异常，避免影响主要功能
            }
        }
    }
}
