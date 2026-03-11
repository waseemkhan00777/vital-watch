using Volo.Abp.Domain.Entities;

namespace VitalCare.Abp.Entities;

public class FirstLoginToken : BasicAggregateRoot<Guid>
{
    public Guid UserId { get; set; }
    public string TempTokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    protected FirstLoginToken() { }

    public FirstLoginToken(Guid id) : base(id) { }
}
