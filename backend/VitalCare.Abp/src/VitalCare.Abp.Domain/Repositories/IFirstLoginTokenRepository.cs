using VitalCare.Abp.Entities;
using Volo.Abp.Domain.Repositories;

namespace VitalCare.Abp.Repositories;

public interface IFirstLoginTokenRepository : IRepository<FirstLoginToken, Guid>
{
    Task<FirstLoginToken?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task DeleteExpiredAsync(DateTime before, CancellationToken cancellationToken = default);
}
