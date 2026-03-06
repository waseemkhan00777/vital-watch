using VitalCare.Abp.DTOs;

namespace VitalCare.Abp;

public interface IAlertAppService
{
    Task<IReadOnlyList<AlertDto>> GetListAsync(Guid? patientId, CancellationToken cancellationToken = default);
    Task<AlertDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AlertDto?> UpdateStateAsync(Guid id, string? state, string? clinicalNote, CancellationToken cancellationToken = default);
}
