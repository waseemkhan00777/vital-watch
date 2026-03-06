using System.Linq;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp.EntityFrameworkCore.Repositories;

public class EfCoreUserRepository : EfCoreRepository<VitalCareAbpDbContext, User, Guid>, IUserRepository
{
    public EfCoreUserRepository(IDbContextProvider<VitalCareAbpDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<List<Guid>> GetPatientIdsAsync(CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.Where(u => u.Role == "patient").Select(u => u.Id).ToListAsync(cancellationToken);
    }

    public async Task<List<User>> GetListOrderedByEmailAsync(CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.OrderBy(u => u.Email).ToListAsync(cancellationToken);
    }
}
