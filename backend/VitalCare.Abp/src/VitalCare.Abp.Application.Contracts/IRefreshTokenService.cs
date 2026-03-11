namespace VitalCare.Abp;

public interface IRefreshTokenService
{
    Task<string> IssueAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(string NewRefreshToken, Guid UserId)?> ValidateAndRotateAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CleanupExpiredAsync(CancellationToken cancellationToken = default);
}
