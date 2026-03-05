namespace VitalCare.Api.Data.Entities;

public class AlertRule
{
    public Guid Id { get; set; }
    public string VitalType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // critical | high | moderate | normal
    public string Operator { get; set; } = "above"; // above | below | between
    public decimal? ThresholdMin { get; set; }
    public decimal? ThresholdMax { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
