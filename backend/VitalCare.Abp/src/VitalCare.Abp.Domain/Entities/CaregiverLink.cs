using Volo.Abp.Domain.Entities;

namespace VitalCare.Abp.Entities;

public class CaregiverLink : BasicAggregateRoot<Guid>
{
    public Guid PatientId { get; set; }
    public Guid CaregiverId { get; set; }
    public DateTime ConsentedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }

    public User Patient { get; set; } = null!;
    public User Caregiver { get; set; } = null!;

    protected CaregiverLink() { }

    public CaregiverLink(Guid id) : base(id) { }
}
