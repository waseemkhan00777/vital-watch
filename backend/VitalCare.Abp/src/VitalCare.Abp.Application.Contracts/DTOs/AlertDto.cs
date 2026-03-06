namespace VitalCare.Abp.DTOs;

public record AlertDto(
    string Id,
    string PatientId,
    string VitalType,
    string Severity,
    string State,
    decimal Value,
    decimal? ValueSecondary,
    string Unit,
    string? RuleId,
    DateTime? AcknowledgedAt,
    string? AcknowledgedById,
    DateTime? ResolvedAt,
    string? ResolvedById,
    DateTime SlaDueAt,
    DateTime CreatedAt,
    string? ClinicalNote);
