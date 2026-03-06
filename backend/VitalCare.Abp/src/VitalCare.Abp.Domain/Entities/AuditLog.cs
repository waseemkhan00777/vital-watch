using Volo.Abp.Domain.Entities;

namespace VitalCare.Abp.Entities;

public class AuditLog : BasicAggregateRoot<Guid>
{
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? ResourceId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    protected AuditLog() { }

    public AuditLog(Guid id) : base(id) { }
}
