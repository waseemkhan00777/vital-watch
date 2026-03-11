using VitalCare.Abp.Entities;
using Volo.Abp.Domain.Repositories;

namespace VitalCare.Abp.Repositories;

public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken, Guid>
{
    Task<PasswordResetToken?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<PasswordResetToken?> FindByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task DeleteExpiredAsync(DateTime before, CancellationToken cancellationToken = default);
}
