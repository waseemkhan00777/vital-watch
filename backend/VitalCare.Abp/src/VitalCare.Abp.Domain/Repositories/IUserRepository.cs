using VitalCare.Abp.Entities;
using Volo.Abp.Domain.Repositories;

namespace VitalCare.Abp.Repositories;

public interface IUserRepository : IRepository<User, Guid>
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<List<Guid>> GetPatientIdsAsync(CancellationToken cancellationToken = default);
    Task<List<User>> GetListOrderedByEmailAsync(CancellationToken cancellationToken = default);
}

