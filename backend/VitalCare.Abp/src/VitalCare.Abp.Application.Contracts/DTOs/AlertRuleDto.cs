namespace VitalCare.Abp.DTOs;

public record AlertRuleDto(
    string Id,
    string VitalType,
    string Severity,
    string Operator,
    decimal? ThresholdMin,
    decimal? ThresholdMax,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
