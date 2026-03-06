using Volo.Abp.Users;
using VitalCare.Abp.DTOs;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp;

public class CaregiverLinkAppService : ICaregiverLinkAppService
{
    private readonly ICaregiverLinkRepository _caregiverLinkRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _auditService;

    public CaregiverLinkAppService(
        ICaregiverLinkRepository caregiverLinkRepository,
        ICurrentUser currentUser,
        IAuditService auditService)
    {
        _caregiverLinkRepository = caregiverLinkRepository;
        _currentUser = currentUser;
        _auditService = auditService;
    }

    public async Task<IReadOnlyList<CaregiverLinkDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser.Id == null) return Array.Empty<CaregiverLinkDto>();
        var list = await _caregiverLinkRepository.GetListForUserAsync(_currentUser.Id.Value, cancellationToken);
        return list.Select(c => new CaregiverLinkDto(c.Id.ToString(), c.PatientId.ToString(), c.CaregiverId.ToString(), c.ConsentedAt, c.RevokedAt)).ToList();
    }

    public async Task<CaregiverLinkDto?> CreateAsync(Guid patientId, Guid caregiverId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.Id == null) return null;
        if (caregiverId != _currentUser.Id && patientId != _currentUser.Id) return null;

        var exists = await _caregiverLinkRepository.ExistsActiveLinkAsync(patientId, caregiverId, cancellationToken);
        if (exists) return null;

        var link = new CaregiverLink(Guid.NewGuid())
        {
            PatientId = patientId,
            CaregiverId = caregiverId,
            ConsentedAt = DateTime.UtcNow
        };
        await _caregiverLinkRepository.InsertAsync(link, true, cancellationToken);
        await _auditService.LogAsync(_currentUser.Id, _currentUser.Email, _currentUser.Roles?.FirstOrDefault() ?? "", "caregiver_link", "create", resourceId: link.Id.ToString(), cancellationToken: cancellationToken);
        return new CaregiverLinkDto(link.Id.ToString(), link.PatientId.ToString(), link.CaregiverId.ToString(), link.ConsentedAt, link.RevokedAt);
    }

    public async Task<bool> RevokeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_currentUser.Id == null) return false;
        var link = await _caregiverLinkRepository.GetAsync(id, true, cancellationToken);
        if (link == null || (link.CaregiverId != _currentUser.Id && link.PatientId != _currentUser.Id)) return false;
        link.RevokedAt = DateTime.UtcNow;
        await _caregiverLinkRepository.UpdateAsync(link, true, cancellationToken);
        await _auditService.LogAsync(_currentUser.Id, _currentUser.Email, _currentUser.Roles?.FirstOrDefault() ?? "", "caregiver_link", "revoke", resourceId: id.ToString(), cancellationToken: cancellationToken);
        return true;
    }
}
