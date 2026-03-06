using Volo.Abp.Users;
using VitalCare.Abp.DTOs;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp;

public class AlertAppService : IAlertAppService
{
    private readonly IAlertRepository _alertRepository;
    private readonly ICaregiverLinkRepository _caregiverLinkRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUser _currentUser;

    public AlertAppService(
        IAlertRepository alertRepository,
        ICaregiverLinkRepository caregiverLinkRepository,
        IUserRepository userRepository,
        ICurrentUser currentUser)
    {
        _alertRepository = alertRepository;
        _caregiverLinkRepository = caregiverLinkRepository;
        _userRepository = userRepository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<AlertDto>> GetListAsync(Guid? patientId, CancellationToken cancellationToken = default)
    {
        var allowedPatientIds = await GetAllowedPatientIdsAsync(cancellationToken);
        if (allowedPatientIds == null) return Array.Empty<AlertDto>();

        var filterId = patientId ?? (allowedPatientIds.Count == 1 ? allowedPatientIds.First() : (Guid?)null);
        if (filterId.HasValue && !allowedPatientIds.Contains(filterId.Value))
            return Array.Empty<AlertDto>();

        var list = await _alertRepository.GetListByPatientIdsAsync(allowedPatientIds, filterId, 500, cancellationToken);
        return list.Select(MapToDto).ToList();
    }

    public async Task<AlertDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var allowedPatientIds = await GetAllowedPatientIdsAsync(cancellationToken);
        if (allowedPatientIds == null) return null;
        var alert = await _alertRepository.GetAsync(id, true, cancellationToken);
        if (alert == null || !allowedPatientIds.Contains(alert.PatientId)) return null;
        return MapToDto(alert);
    }

    public async Task<AlertDto?> UpdateStateAsync(Guid id, string? state, string? clinicalNote, CancellationToken cancellationToken = default)
    {
        var allowedPatientIds = await GetAllowedPatientIdsAsync(cancellationToken);
        if (allowedPatientIds == null) return null;
        var alert = await _alertRepository.GetAsync(id, true, cancellationToken);
        if (alert == null || !allowedPatientIds.Contains(alert.PatientId)) return null;

        if (!string.IsNullOrEmpty(state)) alert.State = state;
        if (clinicalNote != null) alert.ClinicalNote = clinicalNote;
        if (state == "acknowledged") { alert.AcknowledgedAt = DateTime.UtcNow; alert.AcknowledgedById = _currentUser.Id; }
        if (state == "resolved") { alert.ResolvedAt = DateTime.UtcNow; alert.ResolvedById = _currentUser.Id; }

        await _alertRepository.UpdateAsync(alert, true, cancellationToken);
        return MapToDto(alert);
    }

    private static AlertDto MapToDto(Alert a) => new(
        a.Id.ToString(), a.PatientId.ToString(), a.VitalType, a.Severity, a.State,
        a.Value, a.ValueSecondary, a.Unit, a.RuleId?.ToString(),
        a.AcknowledgedAt, a.AcknowledgedById?.ToString(), a.ResolvedAt, a.ResolvedById?.ToString(),
        a.SlaDueAt, a.CreatedAt, a.ClinicalNote);

    private async Task<HashSet<Guid>?> GetAllowedPatientIdsAsync(CancellationToken cancellationToken)
    {
        if (_currentUser.Id == null) return null;
        if (_currentUser.Roles?.Contains("admin") == true || _currentUser.Roles?.Contains("clinician") == true)
        {
            var ids = await _userRepository.GetPatientIdsAsync(cancellationToken);
            return ids.ToHashSet();
        }
        if (_currentUser.Roles?.Contains("patient") == true) return new HashSet<Guid> { _currentUser.Id.Value };
        if (_currentUser.Roles?.Contains("caregiver") == true)
        {
            var ids = await _caregiverLinkRepository.GetConsentedPatientIdsForCaregiverAsync(_currentUser.Id.Value, cancellationToken);
            return ids.ToHashSet();
        }
        return null;
    }
}
