using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostService.DTOs;
using PostService.Services;

namespace PostService.Controllers
{
    /*[ApiController]
    [Route("api/posts/like")]
    public class LikeController : ControllerBase
    {
        private readonly ILikeService _service;

        public LikeController(ILikeService service)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> LikePost([FromBody] LikeRequest request)
        {
            var userId = request.UserId;
            await _service.ToggleLikeAsync(userId, request);
            return Ok(new { code = 200, msg = (string)null, data = (object)null });
        }
    }*/
}
