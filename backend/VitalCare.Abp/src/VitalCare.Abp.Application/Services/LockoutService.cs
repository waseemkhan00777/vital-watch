using Microsoft.Extensions.Configuration;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp.Services;

public class LockoutService : ILockoutService
{
    private readonly IFailedLoginAttemptRepository _failedLoginAttemptRepository;
    private readonly IConfiguration _configuration;

    public LockoutService(IFailedLoginAttemptRepository failedLoginAttemptRepository, IConfiguration configuration)
    {
        _failedLoginAttemptRepository = failedLoginAttemptRepository;
        _configuration = configuration;
    }

    public async Task<bool> IsLockedOutAsync(string email, CancellationToken cancellationToken = default)
    {
        var lockoutMinutes = _configuration.GetValue("Lockout:LockoutDurationMinutes", 15);
        var since = DateTime.UtcNow.AddMinutes(-lockoutMinutes);
        var count = await _failedLoginAttemptRepository.CountRecentByEmailAsync(email, since, cancellationToken);
        var maxAttempts = _configuration.GetValue("Lockout:MaxFailedAttempts", 5);
        return count >= maxAttempts;
    }

    public async Task RecordFailedAttemptAsync(string email, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var attempt = new FailedLoginAttempt(Guid.NewGuid())
        {
            Email = email,
            IpAddress = ipAddress,
            FailedAt = DateTime.UtcNow
        };
        await _failedLoginAttemptRepository.InsertAsync(attempt, true, cancellationToken);
    }

    public async Task ClearFailedAttemptsAsync(string email, CancellationToken cancellationToken = default)
    {
        await _failedLoginAttemptRepository.DeleteByEmailAsync(email, cancellationToken);
    }
}
