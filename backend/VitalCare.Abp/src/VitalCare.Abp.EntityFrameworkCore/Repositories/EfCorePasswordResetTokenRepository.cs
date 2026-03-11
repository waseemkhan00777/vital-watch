using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp.EntityFrameworkCore.Repositories;

public class EfCorePasswordResetTokenRepository : EfCoreRepository<VitalCareAbpDbContext, PasswordResetToken, Guid>, IPasswordResetTokenRepository
{
    public EfCorePasswordResetTokenRepository(IDbContextProvider<VitalCareAbpDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<PasswordResetToken?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);
    }

    public async Task<PasswordResetToken?> FindByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.Where(t => t.UserId == userId && t.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        await dbContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"PasswordResetTokens\" WHERE \"UserId\" = {0}", userId, cancellationToken);
    }

    public async Task DeleteExpiredAsync(DateTime before, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        await dbContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"PasswordResetTokens\" WHERE \"ExpiresAt\" < {0}", before, cancellationToken);
    }
}
