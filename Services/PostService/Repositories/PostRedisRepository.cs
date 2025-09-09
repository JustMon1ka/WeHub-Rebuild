using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PostService.Repositories
{
    public interface IPostRedisRepository
    {
        Task<List<string>> GetHotKeywordsAsync(int count);
        Task IncrementSearchCountAsync(string keyword);
        Task InsertUnreadNoticeAsync(long? userId, string type, long id);
    }

    public class PostRedisRepository : IPostRedisRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private const string HotSearchKey = "hot-searches";
        private const string UnreadNoticeKey = "unread-notice";

        public PostRedisRepository(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// 从 Redis 获取热门搜索关键词
        /// </summary>
        /// <param name="count">返回的关键词数量</param>
        /// <returns>热门关键词列表</returns>
        public async Task<List<string>> GetHotKeywordsAsync(int count)
        {
            var db = _redis.GetDatabase();
            // 使用 Sorted Set 来存储热门搜索，分数是搜索次数
            var topKeywords = await db.SortedSetRangeByRankWithScoresAsync(HotSearchKey, 0, count - 1, Order.Descending);
            return topKeywords.Select(x => x.Element.ToString()).ToList();
        }
    
        /// <summary>
        /// 增加指定关键词的搜索次数
        /// </summary>
        /// <param name="keyword">搜索关键词</param>
        public async Task IncrementSearchCountAsync(string keyword)
        {
            var db = _redis.GetDatabase();
            await db.SortedSetIncrementAsync(HotSearchKey, keyword, 1);
            // 可以设置一个过期时间，防止数据无限增长，比如 30 天
            await db.KeyExpireAsync(HotSearchKey, TimeSpan.FromDays(30));
        }

        /// <summary>
        /// 向指定用户的指定类型未读通知中插入多条消息。
        /// </summary>
        /// <param name="userId">用户的ID。</param>
        /// <param name="type">消息的类型。</param>
        /// <param name="id">消息在数据库中的索引ID。</param>
        public async Task InsertUnreadNoticeAsync(long? userId, string type, long id)
        {
            var db = _redis.GetDatabase();

            // 构建 Redis 列表的键名
            var key = $"{UnreadNoticeKey}:{userId}:{type}";

            // 使用 ListRightPushAsync 将消息ID添加到列表的末尾
            // 这样可以按时间顺序保留消息
            await db.ListRightPushAsync(key, id);
        }
    }
}