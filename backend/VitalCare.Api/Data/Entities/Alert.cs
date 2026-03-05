using System.ComponentModel.DataAnnotations.Schema;

namespace VitalCare.Api.Data.Entities;

public class Alert
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string VitalType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // critical | high | moderate | normal
    public string State { get; set; } = "flagged"; // flagged | acknowledged | escalated | resolved | archived
    public decimal Value { get; set; }
    public decimal? ValueSecondary { get; set; }
    public string Unit { get; set; } = string.Empty;
    public Guid? RuleId { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public Guid? AcknowledgedById { get; set; }
    public DateTime? EscalatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedById { get; set; }
    public DateTime SlaDueAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ClinicalNote { get; set; }

    public User Patient { get; set; } = null!;
    [ForeignKey(nameof(RuleId))]
    public AlertRule? Rule { get; set; }
    public User? AcknowledgedBy { get; set; }
    public User? ResolvedBy { get; set; }
}
