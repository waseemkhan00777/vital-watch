namespace VitalCare.Api.Services;

public interface IAuditService
{
    Task LogAsync(string? userId, string? userEmail, string role, string resource, string action, string? resourceId = null, string? details = null, string? ipAddress = null, CancellationToken ct = default);
}
