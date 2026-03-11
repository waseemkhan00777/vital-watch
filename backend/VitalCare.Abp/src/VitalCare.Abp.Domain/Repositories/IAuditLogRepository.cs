using VitalCare.Abp.Entities;
using Volo.Abp.Domain.Repositories;

namespace VitalCare.Abp.Repositories;

public interface IAuditLogRepository : IRepository<AuditLog, Guid>
{
    Task<List<AuditLog>> GetListAsync(Guid? userId, string? resource, string? resourceId, Guid? patientId, DateTime? from, DateTime? to, int take, CancellationToken cancellationToken = default);
}
