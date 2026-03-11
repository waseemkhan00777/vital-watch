using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp.EntityFrameworkCore.Repositories;

public class EfCoreSessionRepository : EfCoreRepository<VitalCareAbpDbContext, Session, Guid>, ISessionRepository
{
    public EfCoreSessionRepository(IDbContextProvider<VitalCareAbpDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<Session?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.FirstOrDefaultAsync(s => s.TokenHash == tokenHash, cancellationToken);
    }

    public async Task<List<Session>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.Where(s => s.UserId == userId).ToListAsync(cancellationToken);
    }

    public async Task DeleteExpiredAsync(DateTime before, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        await dbContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"Sessions\" WHERE \"ExpiresAt\" < {0}", before, cancellationToken);
    }
}
