using System.Security.Cryptography;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp.Services;

public class CsrfService : ICsrfService
{
    private const int TokenSizeBytes = 32;
    private readonly ISessionRepository _sessionRepository;

    public CsrfService(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<string> GenerateAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var hash = SessionsService.HashToken(accessToken);
        var session = await _sessionRepository.FindByTokenHashAsync(hash, cancellationToken);
        if (session == null)
            throw new InvalidOperationException("Session not found.");
        var tokenBytes = new byte[TokenSizeBytes];
        RandomNumberGenerator.Fill(tokenBytes);
        var tokenHash = Convert.ToHexString(SHA256.HashData(tokenBytes)).ToLowerInvariant();
        session.CsrfTokenHash = tokenHash;
        await _sessionRepository.UpdateAsync(session, cancellationToken: cancellationToken);
        return Convert.ToBase64String(tokenBytes);
    }

    public async Task<bool> ValidateAsync(string accessToken, string? headerValue, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(headerValue)) return false;
        try
        {
            var hash = SessionsService.HashToken(accessToken);
            var session = await _sessionRepository.FindByTokenHashAsync(hash, cancellationToken);
            if (session == null || string.IsNullOrEmpty(session.CsrfTokenHash)) return false;
            var headerBytes = Convert.FromBase64String(headerValue);
            var headerHash = Convert.ToHexString(SHA256.HashData(headerBytes)).ToLowerInvariant();
            return headerHash == session.CsrfTokenHash;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public async Task InvalidateAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var hash = SessionsService.HashToken(accessToken);
        var session = await _sessionRepository.FindByTokenHashAsync(hash, cancellationToken);
        if (session != null)
        {
            session.CsrfTokenHash = null;
            await _sessionRepository.UpdateAsync(session, cancellationToken: cancellationToken);
        }
    }
}
