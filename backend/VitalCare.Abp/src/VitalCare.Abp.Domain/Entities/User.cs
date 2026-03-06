using Volo.Abp.Domain.Entities;

namespace VitalCare.Abp.Entities;

public class User : BasicAggregateRoot<Guid>
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "patient";
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<VitalReading> VitalReadings { get; set; } = new List<VitalReading>();
    public ICollection<Alert> AlertsAcknowledged { get; set; } = new List<Alert>();
    public ICollection<Alert> AlertsResolved { get; set; } = new List<Alert>();
    public ICollection<CaregiverLink> CaregiverLinksAsPatient { get; set; } = new List<CaregiverLink>();
    public ICollection<CaregiverLink> CaregiverLinksAsCaregiver { get; set; } = new List<CaregiverLink>();

    protected User() { }

    public User(Guid id) : base(id) { }
}
