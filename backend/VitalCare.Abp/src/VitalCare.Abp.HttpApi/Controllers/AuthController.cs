using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VitalCare.Abp.DTOs;

namespace VitalCare.Abp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthAppService _authAppService;

    public AuthController(IAuthAppService authAppService)
    {
        _authAppService = authAppService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authAppService.LoginAsync(request, cancellationToken);
        if (result == null)
            return Unauthorized(new { message = "Invalid email or password." });

        Response.Cookies.Append("access_token", result.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            MaxAge = TimeSpan.FromHours(8)
        });
        return Ok(result);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var user = await _authAppService.RegisterAsync(request, cancellationToken);
        if (user == null)
            return BadRequest(new { message = "Email already registered." });
        return Ok(user);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetMe(CancellationToken cancellationToken)
    {
        var user = await _authAppService.GetMeAsync(cancellationToken);
        if (user == null) return Unauthorized();
        return Ok(user);
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("access_token", new CookieOptions { Path = "/" });
        return Ok();
    }
}
