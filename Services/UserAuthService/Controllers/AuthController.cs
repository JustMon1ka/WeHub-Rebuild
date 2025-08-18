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
        var token = await _authService.LoginByEmailCodeAsync(request.Email, request.Code);
        return token == null
            ? Unauthorized(BaseHttpResponse<string>.Fail(401, "验证码错误"))
            : Ok(BaseHttpResponse<string>.Success(token));
    }

}
