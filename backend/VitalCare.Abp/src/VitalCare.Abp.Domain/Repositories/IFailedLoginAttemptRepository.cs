using VitalCare.Abp.Entities;
using Volo.Abp.Domain.Repositories;

namespace VitalCare.Abp.Repositories;

public interface IFailedLoginAttemptRepository : IRepository<FailedLoginAttempt, Guid>
{
    Task<int> CountRecentByEmailAsync(string email, DateTime since, CancellationToken cancellationToken = default);
    Task DeleteByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task DeleteExpiredAsync(DateTime before, CancellationToken cancellationToken = default);
}
