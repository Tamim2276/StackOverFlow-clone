using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackOverflowLite.Application.Features.Auth.Commands;
using StackOverflowLite.Application.Features.Auth.Queries;

namespace StackOverflowLite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    public AuthController(IMediator mediator) => _mediator = mediator;

    //Register a new user and receive a JWT token
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var result = await _mediator.Send(new RegisterCommand(req.Email, req.UserName, req.Password, req.DisplayName));
        if (!result.Succeeded) return BadRequest(new { error = result.Error });
        return Ok(new { token = result.Data });
    }

    //Login with email and password
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var result = await _mediator.Send(new LoginCommand(req.Email, req.Password));
        if (!result.Succeeded) return Unauthorized(new { error = result.Error });
        return Ok(new { token = result.Data });
    }

    //Get the authenticated user's profile
    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();
        var result = await _mediator.Send(new GetProfileQuery(userId));
        if (!result.Succeeded) return NotFound(new { error = result.Error });
        return Ok(result.Data);
    }
}

public record RegisterRequest(string Email, string UserName, string Password, string DisplayName);
public record LoginRequest(string Email, string Password);
