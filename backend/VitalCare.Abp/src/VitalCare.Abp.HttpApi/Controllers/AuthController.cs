using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Hosting;
using VitalCare.Abp.DTOs;

namespace VitalCare.Abp.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthAppService _authAppService;
    private readonly IWebHostEnvironment _env;

    public AuthController(IAuthAppService authAppService, IWebHostEnvironment env)
    {
        _authAppService = authAppService;
        _env = env;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authAppService.LoginAsync(request, cancellationToken);
        if (result == null)
            return Unauthorized(new { message = "Invalid email or password." });

        if (result.Token != null)
        {
            Response.Cookies.Append("access_token", result.Token, GetCookieOptions());
        }
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [Public]
    public async Task<ActionResult<LoginResponse>> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request?.RefreshToken))
            return BadRequest(new { message = "RefreshToken required." });
        var result = await _authAppService.RefreshAsync(request.RefreshToken, cancellationToken);
        if (result == null)
            return Unauthorized(new { message = "Invalid or expired refresh token." });
        if (result.Token != null)
        {
            Response.Cookies.Append("access_token", result.Token, GetCookieOptions());
        }
        return Ok(result);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _authAppService.RegisterAsync(request, cancellationToken);
            if (user == null)
                return BadRequest(new { message = "Email already registered." });
            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetMe(CancellationToken cancellationToken)
    {
        var user = await _authAppService.GetMeAsync(cancellationToken);
        if (user == null) return Unauthorized();
        return Ok(user);
    }

    [HttpGet("csrf-token")]
    [Authorize]
    public async Task<ActionResult<object>> GetCsrfToken(CancellationToken cancellationToken)
    {
        try
        {
            var token = await _authAppService.GetCsrfTokenAsync(cancellationToken);
            return Ok(new { token });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [Public]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await _authAppService.ForgotPasswordAsync(request.Email, cancellationToken);
        return Ok(new { message = "If the email exists, a reset link has been sent." });
    }

    [HttpPost("verify-reset-token")]
    [AllowAnonymous]
    [Public]
    public async Task<ActionResult<object>> VerifyResetToken([FromBody] VerifyResetTokenRequest request, CancellationToken cancellationToken)
    {
        var valid = await _authAppService.VerifyResetTokenAsync(request.Token, cancellationToken);
        return Ok(new { valid });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [Public]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var success = await _authAppService.ResetPasswordAsync(request.Token, request.NewPassword, cancellationToken);
            if (!success)
                return BadRequest(new { message = "Invalid or expired token." });
            return Ok(new { message = "Password reset successfully." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("change-password-first-login")]
    [AllowAnonymous]
    [Public]
    public async Task<ActionResult<LoginResponse>> ChangePasswordFirstLogin([FromBody] ChangePasswordFirstLoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authAppService.ChangePasswordFirstLoginAsync(request.TempToken, request.NewPassword, cancellationToken);
            if (result == null)
                return Unauthorized(new { message = "Invalid or expired token." });
            if (result.Token != null)
            {
                Response.Cookies.Append("access_token", result.Token, GetCookieOptions());
            }
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var token = Request.Headers.Authorization.FirstOrDefault()?.Split(' ', 2).LastOrDefault()
            ?? (Request.Cookies.TryGetValue("access_token", out var c) ? c : null);
        await _authAppService.LogoutAsync(token, cancellationToken);
        Response.Cookies.Delete("access_token", new CookieOptions { Path = "/" });
        return Ok();
    }

    private CookieOptions GetCookieOptions() => new()
    {
        HttpOnly = true,
        Secure = !_env.IsDevelopment(),
        SameSite = SameSiteMode.Strict,
        Path = "/",
        MaxAge = TimeSpan.FromHours(8)
    };
}
