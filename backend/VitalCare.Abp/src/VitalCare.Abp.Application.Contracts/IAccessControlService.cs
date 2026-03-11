namespace VitalCare.Abp;

public interface IAccessControlService
{
    /// <summary>
    /// Async patient access check that enforces the minimum-necessary principle.
    /// Admins pass always. Patients pass for their own record. All other roles
    /// (clinician, care_coordinator, caregiver) must have an active CaregiverLink
    /// to the patient — no blanket clinician access.
    /// </summary>
    Task<bool> CanAccessPatientAsync(string role, Guid userId, Guid patientId, CancellationToken cancellationToken = default);

    bool CanAccessEncounter(string role, Guid userId, Guid? encounterClinicianId, Guid? patientUserId);
    bool CanAccessReferral(string role, Guid userId, Guid? referralClinicianId, Guid? referralCoordinatorId, Guid? patientUserId);
    bool CanModifyEncounter(string role);
    Task<bool> IsCaregiverForPatientAsync(Guid caregiverId, Guid patientId, CancellationToken cancellationToken = default);
    void CheckAccess(bool hasAccess, string resource);
}
