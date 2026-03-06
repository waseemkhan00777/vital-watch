using VitalCare.Abp.DTOs;

namespace VitalCare.Abp;

public interface IAuditAppService
{
    Task<IReadOnlyList<AuditLogDto>> GetListAsync(Guid? userId, string? resource, string? resourceId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
