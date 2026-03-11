namespace VitalCare.Abp.Logging;

/// <summary>
/// Creates PHI-safe loggers for a given category or type.
/// </summary>
public interface IPhisSafeLoggerFactory
{
    IPhisSafeLogger CreateLogger(string categoryName);
    IPhisSafeLogger CreateLogger<T>();
}
