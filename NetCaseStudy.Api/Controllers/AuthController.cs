using Microsoft.AspNetCore.Mvc;
using NetCaseStudy.Application.Abstractions;
using NetCaseStudy.Application.DTOs;

namespace NetCaseStudy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public AuthController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var (success, errors) = await _identityService.RegisterAsync(request.Email, request.Password, request.Role);
        if (!success)
        {
            return BadRequest(new { errors });
        }
        return NoContent();
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (success, token, errors) = await _identityService.LoginAsync(request.Email, request.Password);
        if (!success || token is null)
        {
            return Unauthorized(new { errors });
        }
        return Ok(new { token });
    }
}