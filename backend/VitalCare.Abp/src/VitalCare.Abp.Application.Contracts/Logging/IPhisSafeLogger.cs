namespace VitalCare.Abp.Logging;

/// <summary>
/// Logger that redacts PHI (email, IDs, MRN) from log messages before writing.
/// Use in audit and sensitive services.
/// </summary>
public interface IPhisSafeLogger
{
    void LogTrace(string message, params object[] args);
    void LogDebug(string message, params object[] args);
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogWarning(Exception exception, string message, params object[] args);
    void LogError(string message, params object[] args);
    void LogError(Exception exception, string message, params object[] args);
}
