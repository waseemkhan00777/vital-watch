using System.Data.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace VitalCare.Abp;

/// <summary>
/// EF Core connection interceptor that sets the PostgreSQL session variables
/// app.user_id and app.user_role immediately after a connection is opened.
/// These variables activate the Row-Level Security policies defined in the
/// AddRowLevelSecurity migration, isolating data access per-user at the DB layer.
///
/// Registered as Singleton — safe because IHttpContextAccessor accesses
/// per-request state via AsyncLocal, not at construction time.
/// </summary>
public class RlsSessionInterceptor : DbConnectionInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<RlsSessionInterceptor> _logger;

    private static readonly HashSet<string> KnownRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        VitalCareAbpConstants.Roles.Admin,
        VitalCareAbpConstants.Roles.Clinician,
        VitalCareAbpConstants.Roles.CareCoordinator,
        VitalCareAbpConstants.Roles.Patient,
        VitalCareAbpConstants.Roles.Caregiver
    };

    public RlsSessionInterceptor(IHttpContextAccessor httpContextAccessor, ILogger<RlsSessionInterceptor> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        SetRlsVars(connection);
        base.ConnectionOpened(connection, eventData);
    }

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await SetRlsVarsAsync(connection, cancellationToken);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    private void SetRlsVars(DbConnection connection)
    {
        var (userId, role) = GetCurrentUserInfo();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = BuildSetStatement(userId, role);
        cmd.ExecuteNonQuery();
    }

    private async Task SetRlsVarsAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        var (userId, role) = GetCurrentUserInfo();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = BuildSetStatement(userId, role);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private (string userId, string role) GetCurrentUserInfo()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var rawId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var rawRole = user?.FindFirst(ClaimTypes.Role)?.Value ?? "";

        // Validate to prevent SQL injection: userId must be a valid GUID format
        var userId = Guid.TryParse(rawId, out var parsed) ? parsed.ToString() : "";

        // Validate role against known values
        var role = KnownRoles.Contains(rawRole) ? rawRole.ToLowerInvariant() : "";

        return (userId, role);
    }

    private static string BuildSetStatement(string userId, string role)
    {
        // Values are validated (GUID format / known enum) before interpolation — no injection risk
        return $"SET app.user_id = '{userId}'; SET app.user_role = '{role}';";
    }
}
