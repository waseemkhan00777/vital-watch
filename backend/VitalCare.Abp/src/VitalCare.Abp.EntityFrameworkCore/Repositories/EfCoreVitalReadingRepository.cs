using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp.EntityFrameworkCore.Repositories;

public class EfCoreVitalReadingRepository : EfCoreRepository<VitalCareAbpDbContext, VitalReading, Guid>, IVitalReadingRepository
{
    public EfCoreVitalReadingRepository(IDbContextProvider<VitalCareAbpDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<VitalReading>> GetListByPatientIdsAsync(IEnumerable<Guid> patientIds, Guid? filterPatientId, int take, CancellationToken cancellationToken = default)
    {
        var ids = patientIds.ToHashSet();
        var query = await GetQueryableAsync();
        var q = query.Where(v => ids.Contains(v.PatientId));
        if (filterPatientId.HasValue)
            q = q.Where(v => v.PatientId == filterPatientId.Value);
        return await q.OrderByDescending(v => v.RecordedAt).Take(take).ToListAsync(cancellationToken);
    }
}
