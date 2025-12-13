/*
重构代码：
*/
using MyBackend.Models;

namespace MyBackend.Facades.Interfaces;

public interface IRecommendDbFacade
{
    // 获取个性化推荐话题
    Task<IEnumerable<RecommendResult>> FetchPersonalizedTopicsAsync(int userId, int topK);
    
    // 获取全局热门话题
    Task<IEnumerable<RecommendResult>> FetchGlobalHotTopicsAsync(int topK);
    
    // 获取所有用户的标签画像 (用于计算相似度)
    Task<Dictionary<int, Dictionary<string, int>>> FetchAllUserTagProfilesAsync();
    
    // 获取某用户已关注的人的ID集合
    Task<HashSet<int>> FetchFollowedUserIdsAsync(int userId);
    
    // 批量获取用户详细信息 (根据ID列表)
    Task<IEnumerable<UserProfileResult>> FetchUserDetailsAsync(IEnumerable<int> userIds);
}