using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Volo.Abp.Users;
using VitalCare.Abp.DTOs;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp;

public class AuthAppService : IAuthAppService
{
    private readonly IUserRepository _userRepository;
    private readonly IEncryptionService _encryption;
    private readonly IConfiguration _configuration;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _auditService;

    public AuthAppService(
        IUserRepository userRepository,
        IEncryptionService encryption,
        IConfiguration configuration,
        ICurrentUser currentUser,
        IAuditService auditService)
    {
        _userRepository = userRepository;
        _encryption = encryption;
        _configuration = configuration;
        _currentUser = currentUser;
        _auditService = auditService;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindByEmailAsync(request.Email, cancellationToken);
        if (user == null) return null;
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return null;

        var userDto = MapToDto(user);
        var token = GenerateJwt(userDto);
        await _auditService.LogAsync(user.Id, user.Email, user.Role, "auth", "login", details: "login", cancellationToken: cancellationToken);
        return new LoginResponse(token, userDto);
    }

    public async Task<UserDto?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await _userRepository.FindByEmailAsync(request.Email, cancellationToken) != null)
            return null;

        var user = new User(Guid.NewGuid())
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Name = _encryption.Encrypt(request.Name),
            Role = request.Role,
            CreatedAt = DateTime.UtcNow
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

    public Task LogoutAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    private static UserDto MapToDto(User u, IEncryptionService encryption)
    {
        return new UserDto(u.Id.ToString(), u.Email, u.Role, encryption.Decrypt(u.Name));
    }

    private UserDto MapToDto(User u) => MapToDto(u, _encryption);

    private string GenerateJwt(UserDto user)
    {
        var key = _configuration["Jwt:Key"] ?? "VitalCareSecretKeyAtLeast32CharactersLong!";
        var signingKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(signingKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.Name, user.Name)
        };
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "VitalCare",
            audience: _configuration["Jwt:Audience"] ?? "VitalCare",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
