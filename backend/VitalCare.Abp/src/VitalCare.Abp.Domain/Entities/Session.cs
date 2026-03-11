using Volo.Abp.Domain.Entities;

namespace VitalCare.Abp.Entities;

public class Session : BasicAggregateRoot<Guid>
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public string? CsrfTokenHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime LastActivityAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    protected Session() { }

    public Session(Guid id) : base(id) { }
}
