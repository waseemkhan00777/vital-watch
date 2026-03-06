namespace VitalCare.Abp.DTOs;

public record CaregiverLinkDto(
    string Id,
    string PatientId,
    string CaregiverId,
    DateTime ConsentedAt,
    DateTime? RevokedAt);
