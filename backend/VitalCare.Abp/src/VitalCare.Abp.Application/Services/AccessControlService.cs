using VitalCare.Abp.Repositories;

namespace VitalCare.Abp.Services;

public class AccessControlService : IAccessControlService
{
    private readonly ICaregiverLinkRepository _caregiverLinkRepository;

    public AccessControlService(ICaregiverLinkRepository caregiverLinkRepository)
    {
        _caregiverLinkRepository = caregiverLinkRepository;
    }

    /// <summary>
    /// Enforces minimum-necessary access to a patient record.
    /// - Admin: always granted
    /// - Patient: granted only for their own record (userId == patientId)
    /// - Clinician / CareCoordinator / Caregiver: must have an active CaregiverLink to the patient
    /// </summary>
    public async Task<bool> CanAccessPatientAsync(string role, Guid userId, Guid patientId, CancellationToken cancellationToken = default)
    {
        if (string.Equals(role, VitalCareAbpConstants.Roles.Admin, StringComparison.OrdinalIgnoreCase))
            return true;

        if (string.Equals(role, VitalCareAbpConstants.Roles.Patient, StringComparison.OrdinalIgnoreCase))
            return userId == patientId;

        // Clinician, CareCoordinator, Caregiver: require an active assignment link
        return await _caregiverLinkRepository.ExistsActiveLinkAsync(patientId, userId, cancellationToken);
    }

    public bool CanAccessEncounter(string role, Guid userId, Guid? encounterClinicianId, Guid? patientUserId)
    {
        if (string.Equals(role, VitalCareAbpConstants.Roles.Admin, StringComparison.OrdinalIgnoreCase))
            return true;
        if (patientUserId.HasValue && patientUserId.Value == userId)
            return true;
        if (encounterClinicianId.HasValue && encounterClinicianId.Value == userId && string.Equals(role, VitalCareAbpConstants.Roles.Clinician, StringComparison.OrdinalIgnoreCase))
            return true;
        if (string.Equals(role, VitalCareAbpConstants.Roles.CareCoordinator, StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }

    public bool CanAccessReferral(string role, Guid userId, Guid? referralClinicianId, Guid? referralCoordinatorId, Guid? patientUserId)
    {
        if (string.Equals(role, VitalCareAbpConstants.Roles.Admin, StringComparison.OrdinalIgnoreCase))
            return true;
        if (patientUserId.HasValue && patientUserId.Value == userId)
            return true;
        if (referralClinicianId.HasValue && referralClinicianId.Value == userId)
            return true;
        if (referralCoordinatorId.HasValue && referralCoordinatorId.Value == userId)
            return true;
        return false;
    }

    public bool CanModifyEncounter(string role)
    {
        return string.Equals(role, VitalCareAbpConstants.Roles.Clinician, StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, VitalCareAbpConstants.Roles.Admin, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<bool> IsCaregiverForPatientAsync(Guid caregiverId, Guid patientId, CancellationToken cancellationToken = default)
    {
        return await _caregiverLinkRepository.ExistsActiveLinkAsync(patientId, caregiverId, cancellationToken);
    }

    public void CheckAccess(bool hasAccess, string resource)
    {
        if (!hasAccess)
            throw new UnauthorizedAccessException($"Access denied to {resource}.");
    }
}
