using VitalCare.Abp.DTOs;

namespace VitalCare.Abp;

public interface ICaregiverLinkAppService
{
    Task<IReadOnlyList<CaregiverLinkDto>> GetListAsync(CancellationToken cancellationToken = default);
    Task<CaregiverLinkDto?> CreateAsync(Guid patientId, Guid caregiverId, CancellationToken cancellationToken = default);
    Task<bool> RevokeAsync(Guid id, CancellationToken cancellationToken = default);
}
