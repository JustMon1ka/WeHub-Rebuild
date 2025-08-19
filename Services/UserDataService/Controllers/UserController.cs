using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserDataService.DTOs;
using UserDataService.Services;
using DTOs; 

namespace UserDataService.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserDataService _userService;
        public UserController(IUserDataService userService)
        {
            _userService = userService;
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<BaseHttpResponse<UserInfoResponse>>> GetUser(int id)
        {
            var user = await _userService.GetUserInfoAsync(id);
            if (user == null)
                return NotFound(BaseHttpResponse<UserInfoResponse>.Fail(404, "User not found"));

            return Ok(BaseHttpResponse<UserInfoResponse>.Success(user));
        }
        
        [HttpPut("{id}/user")]
        [Authorize(AuthenticationSchemes = "Bearer")] 
        public async Task<ActionResult<BaseHttpResponse<string>>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId) || userId != id)
                return Unauthorized(BaseHttpResponse<string>.Fail(401, "You are not authorized to modify this user"));
            
            var result = await _userService.UpdateUserAsync(id, request);
            if (!result.Success)
                return BadRequest(BaseHttpResponse<string>.Fail(400, result.Message));

            return Ok(BaseHttpResponse<string>.Success("OK", result.Message));
        }
        
        [HttpPut("{id}/profile")]
        [Authorize(AuthenticationSchemes = "Bearer")] 
        public async Task<ActionResult<BaseHttpResponse<string>>> UpdateUserProfile(int id, [FromBody] UpdateUserInfoRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId) || userId != id)
                return Unauthorized(BaseHttpResponse<string>.Fail(401, "You are not authorized to modify this user"));
            
            var result = await _userService.UpdateUserInfoAsync(id,request);
            if (!result.Success)
                return BadRequest(BaseHttpResponse<string>.Fail(400, result.Message));

            return Ok(BaseHttpResponse<string>.Success("OK", result.Message));
        }

        [HttpDelete("{id}/delete")]
        [Authorize(AuthenticationSchemes = "Bearer")] 
        public async Task<IActionResult> DeleteUser(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId) || userId != id)
                return Unauthorized(BaseHttpResponse<string>.Fail(401, "You are not authorized to modify this user"));

            var result = await _userService.DeleteUserAsync(id);
            if (!result.Success)
                return BadRequest(BaseHttpResponse<string>.Fail(400, result.Message));

            return Ok(BaseHttpResponse<string>.Success("OK", result.Message));
        }

    }
}