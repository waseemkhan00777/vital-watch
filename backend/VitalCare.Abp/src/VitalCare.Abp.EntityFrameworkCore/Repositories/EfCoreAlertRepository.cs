using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp.EntityFrameworkCore.Repositories;

public class EfCoreAlertRepository : EfCoreRepository<VitalCareAbpDbContext, Alert, Guid>, IAlertRepository
{
    public EfCoreAlertRepository(IDbContextProvider<VitalCareAbpDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<Alert>> GetListByPatientIdsAsync(IEnumerable<Guid> patientIds, Guid? filterPatientId, int take, CancellationToken cancellationToken = default)
    {
        var ids = patientIds.ToHashSet();
        var query = await GetQueryableAsync();
        var q = query.Where(a => ids.Contains(a.PatientId));
        if (filterPatientId.HasValue)
            q = q.Where(a => a.PatientId == filterPatientId.Value);
        return await q.OrderByDescending(a => a.CreatedAt).Take(take).ToListAsync(cancellationToken);
    }
}
