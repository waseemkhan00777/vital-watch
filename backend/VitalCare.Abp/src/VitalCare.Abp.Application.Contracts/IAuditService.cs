namespace VitalCare.Abp;

public interface IAuditService
{
    Task LogAsync(Guid? userId, string? userEmail, string role, string resource, string action, string? resourceId = null, string? details = null, string? ipAddress = null, CancellationToken cancellationToken = default);
}
