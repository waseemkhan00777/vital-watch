using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp.EntityFrameworkCore.Repositories;

public class EfCoreRefreshTokenRepository : EfCoreRepository<VitalCareAbpDbContext, RefreshToken, Guid>, IRefreshTokenRepository
{
    public EfCoreRefreshTokenRepository(IDbContextProvider<VitalCareAbpDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<RefreshToken?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.FirstOrDefaultAsync(r => r.TokenHash == tokenHash, cancellationToken);
    }

    public async Task RevokeByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        await dbContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"RefreshTokens\" WHERE \"UserId\" = {0}", userId, cancellationToken);
    }

    public async Task DeleteExpiredAsync(DateTime before, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        await dbContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"RefreshTokens\" WHERE \"ExpiresAt\" < {0}", before, cancellationToken);
    }
}
