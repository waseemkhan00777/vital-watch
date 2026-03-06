using System.Text.Json;
using VitalCare.Abp.DTOs;

namespace VitalCare.Abp;

public interface IVitalReadingAppService
{
    Task<VitalReadingDto?> SubmitVitalAsync(JsonElement body, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VitalReadingDto>> GetListAsync(Guid? patientId, CancellationToken cancellationToken = default);
    Task<VitalReadingDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
}
