namespace VitalCare.Abp;

public interface ISessionsService
{
    Task<Guid> CreateAsync(Guid userId, string accessToken, string? csrfTokenHash, DateTime expiresAt, CancellationToken cancellationToken = default);
    Task<bool> IsSessionValidAsync(string accessToken, CancellationToken cancellationToken = default);
    Task UpdateActivityAsync(string accessToken, CancellationToken cancellationToken = default);
    Task RevokeByTokenAsync(string accessToken, CancellationToken cancellationToken = default);
    Task RevokeByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CleanupExpiredAsync(CancellationToken cancellationToken = default);
}
