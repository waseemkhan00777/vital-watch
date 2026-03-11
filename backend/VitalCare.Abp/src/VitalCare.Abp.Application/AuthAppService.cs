using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Volo.Abp.Users;
using VitalCare.Abp.DTOs;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;
using VitalCare.Abp.Services;

namespace VitalCare.Abp;

public class AuthAppService : IAuthAppService
{
    private readonly IUserRepository _userRepository;
    private readonly IEncryptionService _encryption;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISessionsService _sessionsService;
    private readonly ICsrfService _csrfService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _auditService;
    private readonly IFirstLoginTokenRepository _firstLoginTokenRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IEmailSender _emailSender;
    private readonly ILockoutService _lockoutService;

    public AuthAppService(
        IUserRepository userRepository,
        IEncryptionService encryption,
        IPasswordHasher passwordHasher,
        ISessionsService sessionsService,
        ICsrfService csrfService,
        IRefreshTokenService refreshTokenService,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        ICurrentUser currentUser,
        IAuditService auditService,
        IFirstLoginTokenRepository firstLoginTokenRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IEmailSender emailSender,
        ILockoutService lockoutService)
    {
        _userRepository = userRepository;
        _encryption = encryption;
        _passwordHasher = passwordHasher;
        _sessionsService = sessionsService;
        _csrfService = csrfService;
        _refreshTokenService = refreshTokenService;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _currentUser = currentUser;
        _auditService = auditService;
        _firstLoginTokenRepository = firstLoginTokenRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _emailSender = emailSender;
        _lockoutService = lockoutService;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (await _lockoutService.IsLockedOutAsync(request.Email, cancellationToken))
            return null;
        var user = await _userRepository.FindByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            await _lockoutService.RecordFailedAttemptAsync(request.Email, GetClientIp(), cancellationToken);
            return null;
        }
        if (!user.Active) return null;
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            await _lockoutService.RecordFailedAttemptAsync(request.Email, GetClientIp(), cancellationToken);
            return null;
        }
        await _lockoutService.ClearFailedAttemptsAsync(request.Email, cancellationToken);

        var userDto = MapToDto(user);
        if (user.MustChangePassword)
        {
            var tempToken = await CreateFirstLoginTokenAsync(user.Id, cancellationToken);
            await _auditService.LogAsync(user.Id, user.Email, user.Role, AuditResourceTypes.Auth, AuditActions.Login, null, null, null, null, null, "requires_password_change", null, cancellationToken);
            return new LoginResponse(null, null, null, userDto, RequiresPasswordChange: true, TempToken: tempToken);
        }

        // Enforce 90-day password expiration (HIPAA § 164.308(a)(5))
        if (user.PasswordChangedAt.HasValue &&
            DateTime.UtcNow - user.PasswordChangedAt.Value > TimeSpan.FromDays(90))
        {
            var tempToken = await CreateFirstLoginTokenAsync(user.Id, cancellationToken);
            await _auditService.LogAsync(user.Id, user.Email, user.Role, AuditResourceTypes.Auth, AuditActions.Login, null, null, null, null, null, "password_expired", null, cancellationToken);
            return new LoginResponse(null, null, null, userDto, RequiresPasswordChange: true, TempToken: tempToken);
        }

        var token = GenerateJwt(userDto);
        var sessionExpiryMinutes = _configuration.GetValue("Session:ExpiresInMinutes", 60);
        await _sessionsService.CreateAsync(user.Id, token, null, DateTime.UtcNow.AddMinutes(sessionExpiryMinutes), cancellationToken);
        var csrfToken = await _csrfService.GenerateAsync(token, cancellationToken);
        var refreshToken = await _refreshTokenService.IssueAsync(user.Id, cancellationToken);
        await _auditService.LogLoginAsync(user.Id, user.Email, user.Role, cancellationToken);
        return new LoginResponse(token, refreshToken, csrfToken, userDto);
    }

    public async Task<UserDto?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (!PasswordValidator.IsPasswordComplex(request.Password))
            throw new ArgumentException(PasswordValidator.ComplexityMessage);
        if (await _userRepository.FindByEmailAsync(request.Email, cancellationToken) != null)
            return null;

        var user = new User(Guid.NewGuid())
        {
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Name = _encryption.Encrypt(request.Name),
            Role = request.Role,
            CreatedAt = DateTime.UtcNow,
            Active = true,
            MustChangePassword = false
        };
        await _userRepository.InsertAsync(user, true, cancellationToken);
        return MapToDto(user);
    }

    public async Task<UserDto?> GetMeAsync(CancellationToken cancellationToken = default)
    {
        var id = _currentUser.Id;
        if (id == null) return null;
        var user = await _userRepository.GetAsync(id.Value, true, cancellationToken);
        return user == null ? null : MapToDto(user);
    }

    public async Task LogoutAsync(string? accessToken, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(accessToken))
            await _sessionsService.RevokeByTokenAsync(accessToken, cancellationToken);
    }

    public async Task<string> GetCsrfTokenAsync(CancellationToken cancellationToken = default)
    {
        var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault()?.Split(' ', 2).LastOrDefault()
            ?? (_httpContextAccessor.HttpContext?.Request.Cookies.TryGetValue("access_token", out var c) == true ? c : null);
        if (string.IsNullOrEmpty(token))
            throw new UnauthorizedAccessException("Not authenticated.");
        return await _csrfService.GenerateAsync(token, cancellationToken);
    }

    public async Task<LoginResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var rotated = await _refreshTokenService.ValidateAndRotateAsync(refreshToken, cancellationToken);
        if (rotated == null) return null;
        var (newRefreshToken, userId) = rotated.Value;
        var user = await _userRepository.GetAsync(userId, true, cancellationToken);
        if (user == null || !user.Active) return null;
        var userDto = MapToDto(user);
        var accessToken = GenerateJwt(userDto);
        var sessionExpiryMinutes = _configuration.GetValue("Session:ExpiresInMinutes", 60);
        await _sessionsService.CreateAsync(user.Id, accessToken, null, DateTime.UtcNow.AddMinutes(sessionExpiryMinutes), cancellationToken);
        var csrfToken = await _csrfService.GenerateAsync(accessToken, cancellationToken);
        return new LoginResponse(accessToken, newRefreshToken, csrfToken, userDto);
    }

    public async Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindByEmailAsync(email, cancellationToken);
        if (user == null) return;
        await _passwordResetTokenRepository.DeleteByUserIdAsync(user.Id, cancellationToken);
        var rawToken = GenerateSecureToken();
        var tokenHash = SessionsService.HashToken(rawToken);
        var prt = new PasswordResetToken(Guid.NewGuid())
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow
        };
        await _passwordResetTokenRepository.InsertAsync(prt, true, cancellationToken);
        _ = Task.Run(() => _emailSender.SendPasswordResetAsync(user.Email, rawToken, default));
    }

    public async Task<bool> VerifyResetTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var hash = SessionsService.HashToken(token);
        var prt = await _passwordResetTokenRepository.FindByTokenHashAsync(hash, cancellationToken);
        return prt != null && prt.ExpiresAt > DateTime.UtcNow;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default)
    {
        if (!PasswordValidator.IsPasswordComplex(newPassword))
            throw new ArgumentException(PasswordValidator.ComplexityMessage);
        var hash = SessionsService.HashToken(token);
        var prt = await _passwordResetTokenRepository.FindByTokenHashAsync(hash, cancellationToken);
        if (prt == null || prt.ExpiresAt < DateTime.UtcNow) return false;
        var user = await _userRepository.GetAsync(prt.UserId, true, cancellationToken);
        if (user == null) return false;
        user.PasswordHash = _passwordHasher.HashPassword(newPassword);
        user.MustChangePassword = false;
        user.PasswordChangedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken: cancellationToken);
        await _passwordResetTokenRepository.DeleteAsync(prt, cancellationToken: cancellationToken);
        return true;
    }

    public async Task<LoginResponse?> ChangePasswordFirstLoginAsync(string tempToken, string newPassword, CancellationToken cancellationToken = default)
    {
        if (!PasswordValidator.IsPasswordComplex(newPassword))
            throw new ArgumentException(PasswordValidator.ComplexityMessage);
        var hash = SessionsService.HashToken(tempToken);
        var flt = await _firstLoginTokenRepository.FindByTokenHashAsync(hash, cancellationToken);
        if (flt == null || flt.ExpiresAt < DateTime.UtcNow) return null;
        var user = await _userRepository.GetAsync(flt.UserId, true, cancellationToken);
        if (user == null || !user.Active) return null;
        user.PasswordHash = _passwordHasher.HashPassword(newPassword);
        user.MustChangePassword = false;
        user.PasswordChangedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken: cancellationToken);
        await _firstLoginTokenRepository.DeleteAsync(flt, cancellationToken: cancellationToken);
        var userDto = MapToDto(user);
        var accessToken = GenerateJwt(userDto);
        var sessionExpiryMinutes = _configuration.GetValue("Session:ExpiresInMinutes", 60);
        await _sessionsService.CreateAsync(user.Id, accessToken, null, DateTime.UtcNow.AddMinutes(sessionExpiryMinutes), cancellationToken);
        var csrfToken = await _csrfService.GenerateAsync(accessToken, cancellationToken);
        var refreshToken = await _refreshTokenService.IssueAsync(user.Id, cancellationToken);
        return new LoginResponse(accessToken, refreshToken, csrfToken, userDto);
    }

    private async Task<string> CreateFirstLoginTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        var rawToken = GenerateSecureToken();
        var hash = SessionsService.HashToken(rawToken);
        var flt = new FirstLoginToken(Guid.NewGuid())
        {
            UserId = userId,
            TempTokenHash = hash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            CreatedAt = DateTime.UtcNow
        };
        await _firstLoginTokenRepository.InsertAsync(flt, true, cancellationToken);
        return rawToken;
    }

    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private string? GetClientIp()
    {
        return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
    }

    private static UserDto MapToDto(User u, IEncryptionService encryption)
    {
        return new UserDto(u.Id.ToString(), u.Email, u.Role, encryption.Decrypt(u.Name));
    }

    private UserDto MapToDto(User u) => MapToDto(u, _encryption);

    private string GenerateJwt(UserDto user)
    {
        var key = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("JWT signing key is not configured. Set the Jwt:Key environment variable (e.g. JWT__Key).");
        var signingKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(signingKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.Name, user.Name)
        };
        var expiresIn = TimeSpan.FromHours(1);
        if (TimeSpan.TryParse(_configuration["Jwt:ExpiresIn"], out var parsed))
            expiresIn = parsed;
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "VitalCare",
            audience: _configuration["Jwt:Audience"] ?? "VitalCare",
            claims: claims,
            expires: DateTime.UtcNow.Add(expiresIn),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
