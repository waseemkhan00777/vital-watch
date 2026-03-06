using Microsoft.Extensions.Logging;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp.Services;

public class AlertEvaluationService
{
    private readonly IAlertRuleRepository _ruleRepository;
    private readonly IAlertRepository _alertRepository;
    private readonly ILogger<AlertEvaluationService> _logger;

    private static readonly TimeSpan SlaCritical = TimeSpan.FromHours(1);
    private static readonly TimeSpan SlaHigh = TimeSpan.FromHours(4);
    private static readonly TimeSpan SlaOther = TimeSpan.FromHours(24);

    public AlertEvaluationService(IAlertRuleRepository ruleRepository, IAlertRepository alertRepository, ILogger<AlertEvaluationService> logger)
    {
        _ruleRepository = ruleRepository;
        _alertRepository = alertRepository;
        _logger = logger;
    }

    public async Task EvaluateAsync(VitalReading reading, CancellationToken cancellationToken = default)
    {
        var rules = await _ruleRepository.GetActiveByVitalTypeAsync(reading.Type, cancellationToken);
        foreach (var rule in rules)
        {
            if (!Matches(reading, rule)) continue;
            var slaDue = rule.Severity switch
            {
                "critical" => DateTime.UtcNow.Add(SlaCritical),
                "high" => DateTime.UtcNow.Add(SlaHigh),
                _ => DateTime.UtcNow.Add(SlaOther)
            };
            var alert = new Alert(Guid.NewGuid())
            {
                PatientId = reading.PatientId,
                VitalType = reading.Type,
                Severity = rule.Severity,
                State = "flagged",
                Value = reading.Value,
                ValueSecondary = reading.ValueSecondary,
                Unit = reading.Unit,
                RuleId = rule.Id,
                SlaDueAt = slaDue,
                CreatedAt = DateTime.UtcNow
            };
            await _alertRepository.InsertAsync(alert, true, cancellationToken);
            _logger.LogInformation("Alert created for patient {PatientId}, rule {RuleId}, severity {Severity}", reading.PatientId, rule.Id, rule.Severity);
        }
    }

    private static bool Matches(VitalReading reading, AlertRule rule)
    {
        return rule.Operator switch
        {
            "above" => rule.ThresholdMin.HasValue && reading.Value > rule.ThresholdMin.Value,
            "below" => rule.ThresholdMax.HasValue && reading.Value < rule.ThresholdMax.Value,
            "between" => rule.ThresholdMin.HasValue && rule.ThresholdMax.HasValue &&
                reading.Value >= rule.ThresholdMin.Value && reading.Value <= rule.ThresholdMax.Value,
            _ => false
        };
    }
}
