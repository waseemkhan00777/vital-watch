namespace VitalCare.Api.Data.Entities;

public class VitalReading
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string Type { get; set; } = string.Empty; // blood_pressure | heart_rate | blood_glucose | oxygen_saturation | weight
    public decimal Value { get; set; }
    public decimal? ValueSecondary { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public string Source { get; set; } = "manual"; // manual | system
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Patient { get; set; } = null!;
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
