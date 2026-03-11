using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp.Services;

/// <summary>
/// Background service that periodically deletes expired sessions, refresh tokens,
/// and password-reset tokens. Prevents unbounded growth of these tables and ensures
/// that stale credentials cannot be replayed.
/// Runs every hour; each pass deletes records whose expiry is in the past.
/// </summary>
public class TokenCleanupBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<TokenCleanupBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Token cleanup service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Interval, stoppingToken);

            try
            {
                await CleanupAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Token cleanup failed. Will retry in {Interval}.", Interval);
            }
        }

        logger.LogInformation("Token cleanup service stopped.");
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow;
        using var scope = scopeFactory.CreateScope();

        var sessions = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
        var refreshTokens = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
        var resetTokens = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenRepository>();

        await sessions.DeleteExpiredAsync(cutoff, cancellationToken);
        await refreshTokens.DeleteExpiredAsync(cutoff, cancellationToken);
        await resetTokens.DeleteExpiredAsync(cutoff, cancellationToken);

        logger.LogInformation("Token cleanup completed (cutoff: {Cutoff:u}).", cutoff);
    }
}
