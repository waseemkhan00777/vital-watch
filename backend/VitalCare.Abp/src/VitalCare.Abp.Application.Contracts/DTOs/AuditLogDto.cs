namespace VitalCare.Abp.DTOs;

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
    DateTime Timestamp);
