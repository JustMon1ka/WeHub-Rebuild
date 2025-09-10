using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FollowService.Services;
using FollowService.DTOs;

namespace FollowService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FollowsController : ControllerBase
    {
        private readonly IFollowService _followService;

        public FollowsController(IFollowService followService)
        {
            _followService = followService;
        }

        [HttpPost]
        public async Task<IActionResult> Follow([FromBody] CreateFollowDto createFollowDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _followService.FollowAsync(createFollowDto);
            if (result == null)
            {
                return BadRequest("无法创建关注关系");
            }

            return CreatedAtAction(nameof(Follow), new { followeeId = result.FolloweeId }, result);
        }

        [HttpDelete("{followeeId}")]
        public async Task<IActionResult> Unfollow(int followeeId)
        {
            var result = await _followService.UnfollowAsync(followeeId);
            if (!result)
            {
                return NotFound("关注关系不存在");
            }

            return NoContent();
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetFollowCounts()
        {
            var result = await _followService.GetFollowCountsAsync();
            return Ok(result);
        }

        [HttpGet("following")]
        public async Task<IActionResult> GetFollowing([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest("分页参数无效");
            }

            var result = await _followService.GetFollowingListAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("followers")]
        public async Task<IActionResult> GetFollowers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest("分页参数无效");
            }

            var result = await _followService.GetFollowersListAsync(page, pageSize);
            return Ok(result);
        }
    }
}