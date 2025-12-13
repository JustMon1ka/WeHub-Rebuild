/*
源代码：
using Microsoft.AspNetCore.Mvc;
using MyBackend.Services;

namespace MyBackend.Controllers;

[ApiController]
[Route("recommend")]
public class RecommendController : ControllerBase
{
    private readonly RecommendService _service;
    public RecommendController(RecommendService service) => _service = service;

    [HttpGet("topics")]
    public IActionResult GetTopics([FromQuery]int user_id=1, [FromQuery]int top_k=4)
        => Ok(new { user_id, topics = _service.RecommendTopics(user_id, top_k) });

    [HttpGet("hot")]
    public IActionResult GetHot([FromQuery]int top_k=3)
        => Ok(new { hots = _service.RecommendHot(top_k) });


    [HttpGet("users")]
    public IActionResult GetUsers([FromQuery]int user_id=1, [FromQuery]int top_n=2)
        => Ok(new { user_id, recommended_users = _service.RecommendUsers(user_id, top_n) });
}
*/
/*
重构代码：
*/
using Microsoft.AspNetCore.Mvc;
using MyBackend.Services;

namespace MyBackend.Controllers;

[ApiController]
[Route("recommend")]
public class RecommendController : ControllerBase
{
    private readonly RecommendService _service;

    // 注入业务逻辑服务
    public RecommendController(RecommendService service)
    {
        _service = service;
    }

    // GET: /recommend/topics?user_id=1&top_k=4
    [HttpGet("topics")]
    public async Task<IActionResult> GetTopics([FromQuery] int user_id = 1, [FromQuery] int top_k = 4)
    {
        var topics = await _service.RecommendTopicsAsync(user_id, top_k);
        return Ok(new { user_id, topics });
    }

    // GET: /recommend/hot?top_k=3
    [HttpGet("hot")]
    public async Task<IActionResult> GetHot([FromQuery] int top_k = 3)
    {
        var hots = await _service.RecommendHotAsync(top_k);
        return Ok(new { hots });
    }

    // GET: /recommend/users?user_id=1&top_n=2
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] int user_id = 1, [FromQuery] int top_n = 2)
    {
        var users = await _service.RecommendUsersAsync(user_id, top_n);
        return Ok(new { user_id, recommended_users = users });
    }
}