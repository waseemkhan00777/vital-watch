using Volo.Abp.Domain.Entities;

namespace VitalCare.Abp.Entities;

public class RefreshToken : BasicAggregateRoot<Guid>
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    protected RefreshToken() { }

    public RefreshToken(Guid id) : base(id) { }
}
