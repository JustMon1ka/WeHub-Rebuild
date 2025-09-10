using PostService.Repositories;

namespace PostService.Services
{
    public interface IShareService
    {
        Task SharePostAsync(long sharerId, long targetPostId);
    }

    public class ShareService : IShareService
    {
        private readonly IPostRepository _postRepository;
        private readonly IPostRedisRepository _redisRepository;

        public ShareService(
            IPostRepository postRepository,
            IPostRedisRepository redisRepository)
        {
            _postRepository = postRepository;
            _redisRepository = redisRepository;
        }

        public async Task SharePostAsync(long sharerId, long targetPostId)
        {
            // 找到被分享的原帖
            var post = await _postRepository.GetByIdAsync(targetPostId);
            if (post == null || post.IsDeleted == 1)
            {
                throw new Exception("原帖不存在或已被删除");
            }

            // 插入 Redis 通知
            if (post.UserId.HasValue)
            {
                await _redisRepository.InsertUnreadNoticeAsync(post.UserId, "share", targetPostId);
            }
        }
    }
}
