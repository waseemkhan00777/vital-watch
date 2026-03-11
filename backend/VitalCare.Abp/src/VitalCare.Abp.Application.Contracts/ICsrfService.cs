namespace VitalCare.Abp;

public interface ICsrfService
{
    Task<string> GenerateAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<bool> ValidateAsync(string accessToken, string? headerValue, CancellationToken cancellationToken = default);
    Task InvalidateAsync(string accessToken, CancellationToken cancellationToken = default);
}
