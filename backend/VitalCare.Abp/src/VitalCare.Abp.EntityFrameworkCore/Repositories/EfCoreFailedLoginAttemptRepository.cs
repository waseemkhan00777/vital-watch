using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp.EntityFrameworkCore.Repositories;

public class EfCoreFailedLoginAttemptRepository : EfCoreRepository<VitalCareAbpDbContext, FailedLoginAttempt, Guid>, IFailedLoginAttemptRepository
{
    public EfCoreFailedLoginAttemptRepository(IDbContextProvider<VitalCareAbpDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<int> CountRecentByEmailAsync(string email, DateTime since, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.CountAsync(f => f.Email == email && f.FailedAt >= since, cancellationToken);
    }

    public async Task DeleteByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        await dbContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"FailedLoginAttempts\" WHERE \"Email\" = {0}", email, cancellationToken);
    }

    public async Task DeleteExpiredAsync(DateTime before, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        await dbContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"FailedLoginAttempts\" WHERE \"FailedAt\" < {0}", before, cancellationToken);
    }
}
