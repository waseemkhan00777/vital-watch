namespace VitalCare.Abp.Services;

public class NoOpEmailSender : IEmailSender
{
    public Task SendPasswordResetAsync(string email, string resetLinkOrToken, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
