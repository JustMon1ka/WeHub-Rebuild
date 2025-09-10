using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostService.DTOs;
using PostService.Services;

namespace PostService.Controllers
{
    [ApiController]
    [Route("api/posts/favorite")]
    public class FavoriteController : ControllerBase
    {
        private readonly FavoriteService _service;

        public FavoriteController(FavoriteService service)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite([FromBody] FavoriteRequest request)
        {
            var userId = int.Parse(User.FindFirst("id")?.Value ?? "0");
            await _service.ToggleFavoriteAsync(userId, request.PostId);
            return Ok(new { code = 200, msg = (string)null, data = (object)null });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            var userId = int.Parse(User.FindFirst("id")?.Value ?? "0");
            var list = await _service.GetFavoritesAsync(userId);
            return Ok(new { code = 200, msg = (string)null, data = list });
        }
    }
}
