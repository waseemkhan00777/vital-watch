using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp.EntityFrameworkCore.Repositories;

public class EfCoreFirstLoginTokenRepository : EfCoreRepository<VitalCareAbpDbContext, FirstLoginToken, Guid>, IFirstLoginTokenRepository
{
    public EfCoreFirstLoginTokenRepository(IDbContextProvider<VitalCareAbpDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<FirstLoginToken?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.FirstOrDefaultAsync(t => t.TempTokenHash == tokenHash, cancellationToken);
    }

    public async Task DeleteExpiredAsync(DateTime before, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        await dbContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"FirstLoginTokens\" WHERE \"ExpiresAt\" < {0}", before, cancellationToken);
    }
}
