using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DTOs;
using UserAuthService.DTOs;
using UserAuthService.Services.Interfaces;

namespace UserAuthService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<BaseHttpResponse<string>>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.Success)
            return BadRequest(BaseHttpResponse<string>.Fail(400, result.Message));

        return Ok(BaseHttpResponse<string>.Success("OK", result.Message));
    }

    [HttpPost("login")]
    public async Task<ActionResult<BaseHttpResponse<string>>> Login([FromBody] LoginRequest request)
    {
        var token = await _authService.LoginAsync(request);
        if (token == null)
            return Unauthorized(BaseHttpResponse<string>.Fail(401, "Invalid credentials"));

        return Ok(BaseHttpResponse<string>.Success(token , "Login successful"));
    }
    
    [HttpGet("refresh-token")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult<BaseHttpResponse<string>>> RefreshToken()
    {
        var username = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;
        if (username == null)
            return Unauthorized(BaseHttpResponse<string>.Fail(401, "Invalid token"));
        
        var token = await _authService.RefreshTokenAsync(username);
        if (token == null)
            return Unauthorized(BaseHttpResponse<string>.Fail(401, "Invalid token"));
        return Ok(BaseHttpResponse<string>.Success(token , "Refresh token successful"));
    }

    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public ActionResult<BaseHttpResponse<object>> Me()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var username = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;

        var data = new { id, username };
        return Ok(BaseHttpResponse<object>.Success(data, "User info retrieved"));
    }
    
    [HttpPost("send-code-email")]
    public async Task<IActionResult> SendEmailCode([FromBody] SendEmailCodeRequest request)
    {
        var result = await _authService.SendEmailCodeAsync(request.Email);
        return result.Success
            ? Ok(BaseHttpResponse<string>.Success("OK", result.Message))
            : BadRequest(BaseHttpResponse<string>.Fail(400, result.Message));
    }

    [HttpPost("login-email-code")]
    public async Task<IActionResult> LoginByEmailCode([FromBody] LoginByEmailCodeRequest request)
    {
        var result = await _authService.LoginByEmailCodeAsync(request.Email, request.Code);
        if (!result.Success)
            return Unauthorized(BaseHttpResponse<string>.Fail(401, result.Message));
        if (string.IsNullOrEmpty(result.data))
            return NotFound(BaseHttpResponse<string>.Fail(404, "Login failed, no token generated"));
        return Ok(BaseHttpResponse<string>.Success(result.data ?? "", result.Message));
    }

}
