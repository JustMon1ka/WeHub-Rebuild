using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostService.DTOs;
using PostService.Services;

namespace PostService.Controllers
{
    [ApiController]
    [Route("api/posts/like")]
    public class LikeController : ControllerBase
    {
        private readonly LikeService _service;

        public LikeController(LikeService service)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> LikePost([FromBody] LikeRequest request)
        {
            var userId = int.Parse(User.FindFirst("id")?.Value ?? "0");
            await _service.ToggleLikeAsync(userId, request);
            return Ok(new { code = 200, msg = (string)null, data = (object)null });
        }
    }
}
