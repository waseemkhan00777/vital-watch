using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp.EntityFrameworkCore.Repositories;

public class EfCoreAuditLogRepository : EfCoreRepository<VitalCareAbpDbContext, AuditLog, Guid>, IAuditLogRepository
{
    public EfCoreAuditLogRepository(IDbContextProvider<VitalCareAbpDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<AuditLog>> GetListAsync(Guid? userId, string? resource, string? resourceId, DateTime? from, DateTime? to, int take, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        var q = query.AsQueryable();
        if (userId.HasValue) q = q.Where(a => a.UserId == userId);
        if (!string.IsNullOrEmpty(resource)) q = q.Where(a => a.Resource == resource);
        if (!string.IsNullOrEmpty(resourceId)) q = q.Where(a => a.ResourceId == resourceId);
        if (from.HasValue) q = q.Where(a => a.Timestamp >= from.Value);
        if (to.HasValue) q = q.Where(a => a.Timestamp <= to.Value);
        return await q.OrderByDescending(a => a.Timestamp).Take(take).ToListAsync(cancellationToken);
    }
}
