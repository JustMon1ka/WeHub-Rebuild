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
    }

    public class PostRedisRepository : IPostRedisRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private const string HotSearchKey = "hot-searches";

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
    }
}