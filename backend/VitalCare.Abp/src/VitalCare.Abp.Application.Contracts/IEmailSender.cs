namespace VitalCare.Abp;

public interface IEmailSender
{
    Task SendPasswordResetAsync(string email, string resetLinkOrToken, CancellationToken cancellationToken = default);
}
