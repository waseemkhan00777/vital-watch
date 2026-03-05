using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

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
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.UsePostgreSql();
        });
    }
}
