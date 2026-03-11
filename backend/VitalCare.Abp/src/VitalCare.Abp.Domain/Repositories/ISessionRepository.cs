using VitalCare.Abp.Entities;
using Volo.Abp.Domain.Repositories;

namespace VitalCare.Abp.Repositories;

public interface ISessionRepository : IRepository<Session, Guid>
{
    Task<Session?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<List<Session>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task DeleteExpiredAsync(DateTime before, CancellationToken cancellationToken = default);
}
