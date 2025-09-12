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
