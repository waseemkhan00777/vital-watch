using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp.EntityFrameworkCore.Repositories;

public class EfCoreCaregiverLinkRepository : EfCoreRepository<VitalCareAbpDbContext, CaregiverLink, Guid>, ICaregiverLinkRepository
{
    public EfCoreCaregiverLinkRepository(IDbContextProvider<VitalCareAbpDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<Guid>> GetConsentedPatientIdsForCaregiverAsync(Guid caregiverId, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.Where(c => c.CaregiverId == caregiverId && c.RevokedAt == null).Select(c => c.PatientId).ToListAsync(cancellationToken);
    }

    public async Task<List<CaregiverLink>> GetListForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.Where(c => c.CaregiverId == userId || c.PatientId == userId).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsActiveLinkAsync(Guid patientId, Guid caregiverId, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.AnyAsync(c => c.PatientId == patientId && c.CaregiverId == caregiverId && c.RevokedAt == null, cancellationToken);
    }
}
