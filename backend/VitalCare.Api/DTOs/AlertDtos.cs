namespace VitalCare.Api.DTOs;

public record AlertDto(
    string Id,
    string PatientId,
    string? PatientName,
    string VitalType,
    string Severity,
    string State,
    decimal Value,
    decimal? ValueSecondary,
    string Unit,
    string? RuleId,
    string? AcknowledgedAt,
    string? AcknowledgedBy,
    string? EscalatedAt,
    string? ResolvedAt,
    string? ResolvedBy,
    string SlaDueAt,
    string CreatedAt,
    string? ClinicalNote
);

public record AlertRuleDto(
    string Id,
    string VitalType,
    string Severity,
    string Operator,
    decimal? ThresholdMin,
    decimal? ThresholdMax,
    bool IsActive
);

public record UpdateAlertRequest(string? State, string? ClinicalNote);

public record AlertRuleRequest(string VitalType, string Severity, string Operator, decimal? ThresholdMin, decimal? ThresholdMax);
