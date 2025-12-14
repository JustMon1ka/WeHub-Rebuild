/*
重构代码：
*/
using MyBackend.Facades.Interfaces;

namespace MyBackend.Services;

public class TopicService
{
    private readonly ITopicDbFacade _dbFacade;

    public TopicService(ITopicDbFacade dbFacade)
    {
        _dbFacade = dbFacade;
    }

    public async Task<object> GetTopicStatsAsync(string tagName)
    {
        // 调用 Facade 获取数量
        int count = await _dbFacade.FetchTopicPostCountAsync(tagName);

        // 组装业务对象返回
        return new 
        { 
            topic = tagName, 
            count = count 
        };
    }
}