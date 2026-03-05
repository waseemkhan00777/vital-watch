namespace VitalCare.Api.DTOs;

public record AuditLogDto(
    string Id,
    string? UserId,
    string? UserEmail,
    string Role,
    string Resource,
    string Action,
    string? ResourceId,
    string? Details,
    string? IpAddress,
    string Timestamp
);
