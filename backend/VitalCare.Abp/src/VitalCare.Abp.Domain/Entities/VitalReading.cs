using Volo.Abp.Domain.Entities;

namespace VitalCare.Abp.Entities;

public class VitalReading : BasicAggregateRoot<Guid>
{
    public Guid PatientId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal? ValueSecondary { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public string Source { get; set; } = "manual";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Patient { get; set; } = null!;

    protected VitalReading() { }

    public VitalReading(Guid id) : base(id) { }
}
