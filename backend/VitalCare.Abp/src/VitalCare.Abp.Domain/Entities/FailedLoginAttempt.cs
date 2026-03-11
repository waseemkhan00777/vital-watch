using Volo.Abp.Domain.Entities;

namespace VitalCare.Abp.Entities;

public class FailedLoginAttempt : BasicAggregateRoot<Guid>
{
    public string Email { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public DateTime FailedAt { get; set; } = DateTime.UtcNow;

    protected FailedLoginAttempt() { }

    public FailedLoginAttempt(Guid id) : base(id) { }
}
