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
        await LogAsync(userId, userEmail, role, resource, action, null, null, resourceId, null, null, details, ipAddress, cancellationToken);
    }

    public async Task LogAsync(Guid? userId, string? userEmail, string role, string resource, string action, string? resourceType, string? dataType, string? dataId, Guid? patientId, string? accessedFields, string? details, string? ipAddress, CancellationToken cancellationToken = default)
    {
        ipAddress ??= _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        var log = new AuditLog(Guid.NewGuid())
        {
            UserId = userId,
            UserEmail = userEmail,
            Role = role,
            Resource = resource,
            Action = action,
            ResourceId = dataId,
            ResourceType = resourceType,
            DataType = dataType,
            DataId = dataId,
            PatientId = patientId,
            AccessedFields = accessedFields,
            Details = details,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };
        await _auditLogRepository.InsertAsync(log, true, cancellationToken);
    }

    public Task LogPHIAccessAsync(Guid? userId, string? userEmail, string role, string resourceType, string dataId, Guid? patientId, string? accessedFields, CancellationToken cancellationToken = default)
        => LogAsync(userId, userEmail, role, resourceType, AuditActions.View, resourceType, null, dataId, patientId, accessedFields, null, null, cancellationToken);

    public Task LogLoginAsync(Guid? userId, string? userEmail, string role, CancellationToken cancellationToken = default)
        => LogAsync(userId, userEmail, role, AuditResourceTypes.Auth, AuditActions.Login, null, null, null, null, null, "login", null, cancellationToken);

    public Task LogLogoutAsync(Guid? userId, string? userEmail, string role, CancellationToken cancellationToken = default)
        => LogAsync(userId, userEmail, role, AuditResourceTypes.Auth, AuditActions.Logout, null, null, null, null, null, "logout", null, cancellationToken);

    public Task LogAccessDenialAsync(Guid? userId, string? role, string resource, string? ipAddress, CancellationToken cancellationToken = default)
        => LogAsync(userId, null, role ?? "", resource, "ACCESS_DENIED", null, null, null, null, null, "Authorization denied", ipAddress, cancellationToken);
}
