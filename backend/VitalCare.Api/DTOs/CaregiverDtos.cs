namespace VitalCare.Api.DTOs;

public record CaregiverLinkDto(
    string Id,
    string PatientId,
    string CaregiverId,
    string? CaregiverName,
    string ConsentedAt,
    string? RevokedAt
);

public record InviteCaregiverRequest(string CaregiverEmail, string? PatientId = null);

public record ConsentRequest(string Token); // optional: invite token for consent flow
