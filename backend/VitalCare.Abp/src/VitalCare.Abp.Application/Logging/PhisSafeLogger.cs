using Microsoft.Extensions.Logging;

namespace VitalCare.Abp.Logging;

public class PhisSafeLogger : IPhisSafeLogger
{
    private readonly ILogger _inner;

    public PhisSafeLogger(ILoggerFactory factory, string categoryName)
    {
        _inner = factory.CreateLogger(categoryName);
    }

    public void LogTrace(string message, params object[] args)
        => _inner.LogTrace(PhiRedactor.Redact(string.Format(message, args)));

    public void LogDebug(string message, params object[] args)
        => _inner.LogDebug(PhiRedactor.Redact(string.Format(message, args)));

    public void LogInformation(string message, params object[] args)
        => _inner.LogInformation(PhiRedactor.Redact(string.Format(message, args)));

    public void LogWarning(string message, params object[] args)
        => _inner.LogWarning(PhiRedactor.Redact(string.Format(message, args)));

    public void LogWarning(Exception exception, string message, params object[] args)
        => _inner.LogWarning(WrapWithRedactedMessage(exception), PhiRedactor.Redact(string.Format(message, args)));

    public void LogError(string message, params object[] args)
        => _inner.LogError(PhiRedactor.Redact(string.Format(message, args)));

    public void LogError(Exception exception, string message, params object[] args)
        => _inner.LogError(WrapWithRedactedMessage(exception), PhiRedactor.Redact(string.Format(message, args)));

    private static Exception WrapWithRedactedMessage(Exception ex)
        => new Exception(PhiRedactor.Redact(ex.Message), ex);
}
