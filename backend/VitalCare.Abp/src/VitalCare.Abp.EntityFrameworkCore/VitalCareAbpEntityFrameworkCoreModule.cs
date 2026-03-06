using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.Modularity;
using VitalCare.Abp.Entities;
using VitalCare.Abp.EntityFrameworkCore.Repositories;

namespace VitalCare.Abp;

[DependsOn(
    typeof(VitalCareAbpDomainModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule)
)]
public class VitalCareAbpEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<VitalCareAbpDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            options.AddRepository<User, EfCoreUserRepository>();
            options.AddRepository<AlertRule, EfCoreAlertRuleRepository>();
            options.AddRepository<CaregiverLink, EfCoreCaregiverLinkRepository>();
            options.AddRepository<VitalReading, EfCoreVitalReadingRepository>();
            options.AddRepository<Alert, EfCoreAlertRepository>();
            options.AddRepository<AuditLog, EfCoreAuditLogRepository>();
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.UseNpgsql(builder =>
            {
                builder.MigrationsAssembly(typeof(VitalCareAbpEntityFrameworkCoreModule).Assembly.GetName().Name);
            });
        });
    }
}
