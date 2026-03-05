namespace VitalCare.Api.Data.Entities;

public class CaregiverLink
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid CaregiverId { get; set; }
    public DateTime ConsentedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }

    public User Patient { get; set; } = null!;
    public User Caregiver { get; set; } = null!;
}
