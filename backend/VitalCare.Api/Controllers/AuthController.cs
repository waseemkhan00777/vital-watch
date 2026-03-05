using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VitalCare.Api.DTOs;
using VitalCare.Api.Services;

namespace VitalCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly IAuditService _audit;

    public AuthController(IAuthService auth, IAuditService audit)
    {
        _auth = auth;
        _audit = audit;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest? request, CancellationToken ct)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Email and password are required." });
        var result = await _auth.LoginAsync(request, ct);
        if (result == null)
            return Unauthorized(new { message = "Invalid email or password." });
        await _audit.LogAsync(result.User.Id, result.User.Email, result.User.Role, "auth", "login", ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(), ct: ct);

        // HIPAA: store token in httpOnly, Secure, SameSite cookie to avoid XSS exposure
        var isProduction = !HttpContext.Request.Host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase);
        Response.Cookies.Append("access_token", result.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            MaxAge = TimeSpan.FromHours(8)
        });
        return Ok(result);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterRequest? request, CancellationToken ct)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Email, password, and name are required." });
        var user = await _auth.RegisterAsync(request, ct);
        if (user == null)
            return BadRequest(new { message = "Email already registered." });
        await _audit.LogAsync(user.Id, user.Email, user.Role, "user", "register", user.Id, ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(), ct: ct);
        return Ok(user);
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "patient";
        var name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? email ?? "";
        if (string.IsNullOrEmpty(id)) return Unauthorized();
        return Ok(new UserDto(id, email ?? "", role, name));
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "patient";
            await _audit.LogAsync(id ?? "", email, role ?? "", "auth", "logout", ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(), ct: ct);
        }
        Response.Cookies.Delete("access_token", new CookieOptions { Path = "/" });
        return NoContent();
    }
}
