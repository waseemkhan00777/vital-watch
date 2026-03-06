namespace VitalCare.Abp;

public record UpdateAlertRuleInput(
    string? VitalType,
    string? Severity,
    string? Operator,
    decimal? ThresholdMin,
    decimal? ThresholdMax,
    bool? IsActive);
