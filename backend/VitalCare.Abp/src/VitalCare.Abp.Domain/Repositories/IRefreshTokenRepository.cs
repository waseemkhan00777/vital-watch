using VitalCare.Abp.Entities;
using Volo.Abp.Domain.Repositories;

namespace VitalCare.Abp.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken, Guid>
{
    Task<RefreshToken?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task RevokeByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task DeleteExpiredAsync(DateTime before, CancellationToken cancellationToken = default);
}
