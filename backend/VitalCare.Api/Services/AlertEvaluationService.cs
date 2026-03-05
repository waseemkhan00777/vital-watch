using Microsoft.EntityFrameworkCore;
using VitalCare.Api.Data;
using VitalCare.Api.Data.Entities;

namespace VitalCare.Api.Services;

/// <summary>Evaluates vitals against alert rules and creates alerts with SLA (Critical 1h, High 4h).</summary>
public class AlertEvaluationService
{
    private readonly ApplicationDbContext _db;
    private static readonly TimeSpan SlaCritical = TimeSpan.FromHours(1);
    private static readonly TimeSpan SlaHigh = TimeSpan.FromHours(4);

    public AlertEvaluationService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task EvaluateAndCreateAlertsAsync(VitalReading reading, CancellationToken ct = default)
    {
        var rules = await _db.AlertRules
            .Where(r => r.IsActive && r.VitalType == reading.Type)
            .ToListAsync(ct);

        foreach (var rule in rules)
        {
            if (!MatchesRule(reading, rule)) continue;

            var slaDue = rule.Severity switch
            {
                "critical" => DateTime.UtcNow.Add(SlaCritical),
                "high" => DateTime.UtcNow.Add(SlaHigh),
                _ => DateTime.UtcNow.Add(TimeSpan.FromHours(24))
            };

            var alert = new Alert
            {
                Id = Guid.NewGuid(),
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
            _db.Alerts.Add(alert);
        }

        await _db.SaveChangesAsync(ct);
    }

    private static bool MatchesRule(VitalReading r, AlertRule rule)
    {
        switch (rule.Operator)
        {
            case "above":
                return r.Value >= (rule.ThresholdMin ?? 0);
            case "below":
                return r.Value <= (rule.ThresholdMax ?? decimal.MaxValue);
            case "between":
                return r.Value >= (rule.ThresholdMin ?? 0) && r.Value <= (rule.ThresholdMax ?? decimal.MaxValue);
            default:
                return r.Value >= (rule.ThresholdMin ?? 0);
        }
    }
}
