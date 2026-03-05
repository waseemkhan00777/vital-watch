using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VitalCare.Api.Data;
using VitalCare.Api.Data.Entities;
using VitalCare.Api.DTOs;

namespace VitalCare.Api.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;
    private readonly IEncryptionService _encryption;

    public AuthService(ApplicationDbContext db, IConfiguration config, IEncryptionService encryption)
    {
        _db = db;
        _config = config;
        _encryption = encryption;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email, ct);
        if (user == null) return null;
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return null;
        var dto = MapUser(user);
        var token = GenerateJwt(dto);
        return new LoginResponse(token, dto);
    }

    public async Task<UserDto?> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email, ct))
            return null;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Name = _encryption.Encrypt(request.Name),
            Role = request.Role
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return MapUser(user);
    }

    public string GenerateJwt(UserDto user)
    {
        var key = _config["Jwt:Key"] ?? "VitalCareSecretKeyAtLeast32CharactersLong!";
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.Name, user.Name)
        };
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "VitalCare",
            audience: _config["Jwt:Audience"] ?? "VitalCare",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private UserDto MapUser(User u) => new(
        u.Id.ToString(),
        u.Email,
        u.Role,
        _encryption.Decrypt(u.Name)
    );
}
