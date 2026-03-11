using Microsoft.Extensions.Logging;

namespace VitalCare.Abp.Logging;

public class PhisSafeLoggerFactory : IPhisSafeLoggerFactory
{
    private readonly ILoggerFactory _factory;

    public PhisSafeLoggerFactory(ILoggerFactory factory)
    {
        _factory = factory;
    }

    public IPhisSafeLogger CreateLogger(string categoryName)
        => new PhisSafeLogger(_factory, categoryName);

    public IPhisSafeLogger CreateLogger<T>()
        => new PhisSafeLogger(_factory, typeof(T).FullName ?? typeof(T).Name);
}
