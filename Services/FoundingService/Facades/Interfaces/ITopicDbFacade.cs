/*
重构代码：
*/
namespace MyBackend.Facades.Interfaces;

public interface ITopicDbFacade
{
    // 获取指定话题下的有效帖子数量
    Task<int> FetchTopicPostCountAsync(string tagName);
}