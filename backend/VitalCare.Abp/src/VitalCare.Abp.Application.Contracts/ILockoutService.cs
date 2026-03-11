namespace VitalCare.Abp;

public interface ILockoutService
{
    Task<bool> IsLockedOutAsync(string email, CancellationToken cancellationToken = default);
    Task RecordFailedAttemptAsync(string email, string? ipAddress, CancellationToken cancellationToken = default);
    Task ClearFailedAttemptsAsync(string email, CancellationToken cancellationToken = default);
}
