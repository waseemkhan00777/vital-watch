using Microsoft.AspNetCore.Http;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(IAuditLogRepository auditLogRepository, IHttpContextAccessor httpContextAccessor)
    {
        _auditLogRepository = auditLogRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(Guid? userId, string? userEmail, string role, string resource, string action, string? resourceId = null, string? details = null, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        ipAddress ??= _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        var log = new AuditLog(Guid.NewGuid())
        {
            UserId = userId,
            UserEmail = userEmail,
            Role = role,
            Resource = resource,
            Action = action,
            ResourceId = resourceId,
            Details = details,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };
        await _auditLogRepository.InsertAsync(log, true, cancellationToken);
    }
}
