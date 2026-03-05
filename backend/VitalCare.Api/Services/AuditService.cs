using VitalCare.Api.Data;
using VitalCare.Api.Data.Entities;

namespace VitalCare.Api.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _db;

    public AuditService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(string? userId, string? userEmail, string role, string resource, string action, string? resourceId = null, string? details = null, string? ipAddress = null, CancellationToken ct = default)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId),
            UserEmail = userEmail,
            Role = role,
            Resource = resource,
            Action = action,
            ResourceId = resourceId,
            Details = details,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
    }
}
