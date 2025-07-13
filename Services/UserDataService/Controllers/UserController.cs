using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserDataService.DTOs;
using UserDataService.Services;

namespace UserDataService.Controllers
{
    [ApiController]
    [Route("api/user")]
    [Authorize(AuthenticationSchemes = "Bearer")] // 要求用户认证
    public class UserController : ControllerBase
    {
        private readonly IDataService _userService;
        public UserController(IDataService userService)
        {
            _userService = userService;
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid token");

            var success = await _userService.DeleteUserAsync(userId);
            if (!success)
                return NotFound(new { message = "User not found." });

            return Ok(new { message = "User deleted successfully." });
        }

    }
}