namespace VitalCare.Abp;

public record CreateAlertRuleInput(
    string VitalType,
    string Severity,
    string Operator,
    decimal? ThresholdMin,
    decimal? ThresholdMax,
    bool IsActive = true);
