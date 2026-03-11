using Volo.Abp.Domain.Entities;

namespace VitalCare.Abp.Entities;

public class PasswordResetToken : BasicAggregateRoot<Guid>
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    protected PasswordResetToken() { }

    public PasswordResetToken(Guid id) : base(id) { }
}
