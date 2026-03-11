using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp.Services;

public class SessionsService : ISessionsService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IConfiguration _configuration;

    public SessionsService(ISessionRepository sessionRepository, IConfiguration configuration)
    {
        _sessionRepository = sessionRepository;
        _configuration = configuration;
    }

    public async Task<Guid> CreateAsync(Guid userId, string accessToken, string? csrfTokenHash, DateTime expiresAt, CancellationToken cancellationToken = default)
    {
        var session = new Session(Guid.NewGuid())
        {
            UserId = userId,
            TokenHash = HashToken(accessToken),
            CsrfTokenHash = csrfTokenHash,
            ExpiresAt = expiresAt,
            LastActivityAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        await _sessionRepository.InsertAsync(session, true, cancellationToken);
        return session.Id;
    }

    public async Task<bool> IsSessionValidAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var hash = HashToken(accessToken);
        var session = await _sessionRepository.FindByTokenHashAsync(hash, cancellationToken);
        if (session == null) return false;
        if (session.ExpiresAt < DateTime.UtcNow) return false;
        var inactivityMinutes = _configuration.GetValue("Session:InactivityTimeoutMinutes", 30);
        if (session.LastActivityAt.AddMinutes(inactivityMinutes) < DateTime.UtcNow) return false;
        return true;
    }

    public async Task UpdateActivityAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var hash = HashToken(accessToken);
        var session = await _sessionRepository.FindByTokenHashAsync(hash, cancellationToken);
        if (session == null) return;
        session.LastActivityAt = DateTime.UtcNow;
        await _sessionRepository.UpdateAsync(session, cancellationToken: cancellationToken);
    }

    public async Task RevokeByTokenAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var hash = HashToken(accessToken);
        var session = await _sessionRepository.FindByTokenHashAsync(hash, cancellationToken);
        if (session != null)
            await _sessionRepository.DeleteAsync(session, cancellationToken: cancellationToken);
    }

    public async Task RevokeByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var sessions = await _sessionRepository.GetByUserIdAsync(userId, cancellationToken);
        foreach (var s in sessions)
            await _sessionRepository.DeleteAsync(s, cancellationToken: cancellationToken);
    }

    public async Task CleanupExpiredAsync(CancellationToken cancellationToken = default)
    {
        await _sessionRepository.DeleteExpiredAsync(DateTime.UtcNow, cancellationToken);
    }

    public static string HashToken(string token)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
