using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp.EntityFrameworkCore.Repositories;

public class EfCoreAlertRuleRepository : EfCoreRepository<VitalCareAbpDbContext, AlertRule, Guid>, IAlertRuleRepository
{
    public EfCoreAlertRuleRepository(IDbContextProvider<VitalCareAbpDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<AlertRule>> GetActiveByVitalTypeAsync(string vitalType, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.Where(r => r.VitalType == vitalType && r.IsActive).ToListAsync(cancellationToken);
    }

    public async Task<List<AlertRule>> GetListOrderedByVitalTypeAsync(CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.OrderBy(r => r.VitalType).ToListAsync(cancellationToken);
    }
}
