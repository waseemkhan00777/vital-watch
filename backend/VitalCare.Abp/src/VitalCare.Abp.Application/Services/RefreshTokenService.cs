using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private const int TokenSizeBytes = 32;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;

    public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _configuration = configuration;
    }

    public async Task<string> IssueAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var rawToken = GenerateRawToken();
        var hash = HashToken(rawToken);
        var expiresIn = TimeSpan.FromDays(1);
        if (TimeSpan.TryParse(_configuration["Jwt:RefreshExpiresIn"], out var parsed))
            expiresIn = parsed;
        var entity = new RefreshToken(Guid.NewGuid())
        {
            UserId = userId,
            TokenHash = hash,
            ExpiresAt = DateTime.UtcNow.Add(expiresIn),
            CreatedAt = DateTime.UtcNow
        };
        await _refreshTokenRepository.InsertAsync(entity, true, cancellationToken);
        return rawToken;
    }

    public async Task<(string NewRefreshToken, Guid UserId)?> ValidateAndRotateAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var hash = HashToken(refreshToken);
        var entity = await _refreshTokenRepository.FindByTokenHashAsync(hash, cancellationToken);
        if (entity == null || entity.ExpiresAt < DateTime.UtcNow)
            return null;
        var userId = entity.UserId;
        await _refreshTokenRepository.DeleteAsync(entity, cancellationToken: cancellationToken);
        var newToken = await IssueAsync(userId, cancellationToken);
        return (newToken, userId);
    }

    public async Task RevokeByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _refreshTokenRepository.RevokeByUserIdAsync(userId, cancellationToken);
    }

    public async Task CleanupExpiredAsync(CancellationToken cancellationToken = default)
    {
        await _refreshTokenRepository.DeleteExpiredAsync(DateTime.UtcNow, cancellationToken);
    }

    private static string GenerateRawToken()
    {
        var bytes = new byte[TokenSizeBytes];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string HashToken(string token)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }
}
