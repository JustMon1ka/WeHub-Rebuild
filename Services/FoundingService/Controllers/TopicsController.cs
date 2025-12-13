/*
原来代码：
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

[ApiController]
[Route("api/[controller]")]
public class TopicsController : ControllerBase
{
    private readonly IConfiguration _config;

    public TopicsController(IConfiguration config)
    {
        _config = config;
    }

    // GET: /api/topics/{tagName}
    [HttpGet("{tagName}")]
    public IActionResult GetTopicCount(string tagName)
    {
        var connStr = _config.GetConnectionString("DefaultConnection");

        try
        {
            using var conn = new OracleConnection(connStr);
            conn.Open();

            string sql = @"
                SELECT COUNT(p.POST_ID)
                FROM POST p
                JOIN POSTTAG pt ON p.POST_ID = pt.POST_ID
                JOIN TAG t ON pt.TAG_ID = t.TAG_ID
                WHERE t.TAG_NAME = :tagName
                  AND NVL(p.IS_DELETED,0)=0
                  AND NVL(p.IS_HIDDEN,0)=0";

            using var cmd = new OracleCommand(sql, conn);
            cmd.Parameters.Add(new OracleParameter("tagName", tagName));

            var count = Convert.ToInt32(cmd.ExecuteScalar());

            return Ok(new { topic = tagName, count });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
*/
/*
重构代码：
*/
using Microsoft.AspNetCore.Mvc;
using MyBackend.Services;

namespace MyBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TopicsController : ControllerBase
{
    private readonly TopicService _service;

    // 注入话题业务服务
    public TopicsController(TopicService service)
    {
        _service = service;
    }

    // GET: /api/topics/{tagName}
    [HttpGet("{tagName}")]
    public async Task<IActionResult> GetTopicCount(string tagName)
    {
        try 
        {
            // 调用 Service 获取统计数据
            var result = await _service.GetTopicStatsAsync(tagName);
            return Ok(result);
        }
        catch (Exception ex)
        {
            // 简单的异常处理，实际项目中可以使用全局异常过滤器
            return BadRequest(new { error = ex.Message });
        }
    }
}