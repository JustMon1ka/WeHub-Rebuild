using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserTagService.DTOs;
using UserTagService.Services;
using DTOs; // BaseHttpResponse

namespace UserTagService.Controllers;

[ApiController]
[Route("api/users/{id}/tags")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class UserTagsController : ControllerBase
{
    private readonly IUserTagService _tagService;

    public UserTagsController(IUserTagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    public async Task<ActionResult<BaseHttpResponse<UserTagResponse>>> GetTags(int id)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out var userId) || userId != id)
            return Unauthorized(BaseHttpResponse<UserTagResponse>.Fail(401, "Unauthorized"));

        var tags = await _tagService.GetUserTagsAsync(id);
        return Ok(BaseHttpResponse<UserTagResponse>.Success(new UserTagResponse { Tags = tags }));
    }

    [HttpPut]
    public async Task<ActionResult<BaseHttpResponse<string>>> UpdateTags(int id, [FromBody] UpdateUserTagRequest request)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out var userId) || userId != id)
            return Unauthorized(BaseHttpResponse<string>.Fail(401, "Unauthorized"));

        var result = await _tagService.UpdateUserTagsAsync(id, request.Tags);
        if (!result.Success)
            return BadRequest(BaseHttpResponse<string>.Fail(400, "Failed to update tags"));

        return Ok(BaseHttpResponse<string>.Success("OK", result.Message));
    }
}