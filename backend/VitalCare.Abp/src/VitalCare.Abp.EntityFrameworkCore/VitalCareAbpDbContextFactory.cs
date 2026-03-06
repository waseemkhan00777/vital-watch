using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace VitalCare.Abp;

public class VitalCareAbpDbContextFactory : IDesignTimeDbContextFactory<VitalCareAbpDbContext>
{
    public VitalCareAbpDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "VitalCare.Abp.HttpApi.Host");
        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<VitalCareAbpDbContext>();
        var connectionString = config.GetConnectionString("Default") ?? "Host=localhost;Port=5432;Database=VitalCare;Username=postgres;Password=postgres;";
        optionsBuilder.UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsAssembly(typeof(VitalCareAbpDbContextFactory).Assembly.GetName().Name));

        return new VitalCareAbpDbContext(optionsBuilder.Options);
    }
}
