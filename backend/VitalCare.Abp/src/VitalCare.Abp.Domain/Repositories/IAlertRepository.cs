using VitalCare.Abp.Entities;
using Volo.Abp.Domain.Repositories;

namespace VitalCare.Abp.Repositories;

public interface IAlertRepository : IRepository<Alert, Guid>
{
    Task<List<Alert>> GetListByPatientIdsAsync(IEnumerable<Guid> patientIds, Guid? filterPatientId, int take, CancellationToken cancellationToken = default);
}
