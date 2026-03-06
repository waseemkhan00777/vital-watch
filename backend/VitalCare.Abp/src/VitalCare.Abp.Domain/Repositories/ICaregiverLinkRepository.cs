using VitalCare.Abp.Entities;
using Volo.Abp.Domain.Repositories;

namespace VitalCare.Abp.Repositories;

public interface ICaregiverLinkRepository : IRepository<CaregiverLink, Guid>
{
    Task<List<Guid>> GetConsentedPatientIdsForCaregiverAsync(Guid caregiverId, CancellationToken cancellationToken = default);
    Task<List<CaregiverLink>> GetListForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveLinkAsync(Guid patientId, Guid caregiverId, CancellationToken cancellationToken = default);
}
