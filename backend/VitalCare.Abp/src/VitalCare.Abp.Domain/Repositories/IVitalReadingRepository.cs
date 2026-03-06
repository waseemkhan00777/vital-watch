using VitalCare.Abp.Entities;
using Volo.Abp.Domain.Repositories;

namespace VitalCare.Abp.Repositories;

public interface IVitalReadingRepository : IRepository<VitalReading, Guid>
{
    Task<List<VitalReading>> GetListByPatientIdsAsync(IEnumerable<Guid> patientIds, Guid? filterPatientId, int take, CancellationToken cancellationToken = default);
}
