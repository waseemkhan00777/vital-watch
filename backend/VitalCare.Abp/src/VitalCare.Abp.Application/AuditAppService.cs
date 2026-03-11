using Volo.Abp.Users;
using VitalCare.Abp.DTOs;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp;

public class AuditAppService : IAuditAppService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUser _currentUser;

    public AuditAppService(IAuditLogRepository auditLogRepository, ICurrentUser currentUser)
    {
        _auditLogRepository = auditLogRepository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetListAsync(Guid? userId, string? resource, string? resourceId, Guid? patientId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        if (_currentUser.Roles?.Contains("admin") != true && _currentUser.Roles?.Contains("clinician") != true && _currentUser.Roles?.Contains("care_coordinator") != true)
            return Array.Empty<AuditLogDto>();
        if (_currentUser.Roles?.Contains("admin") != true && userId.HasValue && userId != _currentUser.Id)
            return Array.Empty<AuditLogDto>();

        var list = await _auditLogRepository.GetListAsync(userId, resource, resourceId, patientId, from, to, 500, cancellationToken);
        return list.Select(a => new AuditLogDto(a.Id.ToString(), a.UserId?.ToString(), a.UserEmail, a.Role, a.Resource, a.Action, a.ResourceId, a.Details, a.IpAddress, a.Timestamp)).ToList();
    }
}
