using Volo.Abp.Domain.Entities;

namespace VitalCare.Abp.Entities;

public class AlertRule : BasicAggregateRoot<Guid>
{
    public string VitalType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Operator { get; set; } = "above";
    public decimal? ThresholdMin { get; set; }
    public decimal? ThresholdMax { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    protected AlertRule() { }

    public AlertRule(Guid id) : base(id) { }
}
