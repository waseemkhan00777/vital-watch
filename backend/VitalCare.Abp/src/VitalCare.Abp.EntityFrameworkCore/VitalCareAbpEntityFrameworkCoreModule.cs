using Microsoft.EntityFrameworkCore;
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
        // Register the RLS interceptor as Singleton (IHttpContextAccessor is AsyncLocal-safe from Singleton)
        context.Services.AddSingleton<RlsSessionInterceptor>();

        context.Services.AddAbpDbContext<VitalCareAbpDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            options.AddRepository<User, EfCoreUserRepository>();
            options.AddRepository<Session, EfCoreSessionRepository>();
            options.AddRepository<RefreshToken, EfCoreRefreshTokenRepository>();
            options.AddRepository<FirstLoginToken, EfCoreFirstLoginTokenRepository>();
            options.AddRepository<FailedLoginAttempt, EfCoreFailedLoginAttemptRepository>();
            options.AddRepository<PasswordResetToken, EfCorePasswordResetTokenRepository>();
            options.AddRepository<AlertRule, EfCoreAlertRuleRepository>();
            options.AddRepository<CaregiverLink, EfCoreCaregiverLinkRepository>();
            options.AddRepository<VitalReading, EfCoreVitalReadingRepository>();
            options.AddRepository<Alert, EfCoreAlertRepository>();
            options.AddRepository<AuditLog, EfCoreAuditLogRepository>();
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure<VitalCareAbpDbContext>(ctx =>
            {
                // Use ABP's PostgreSQL extension directly on ctx (not ctx.DbContextOptions)
                ctx.UseNpgsql(builder =>
                {
                    builder.MigrationsAssembly(typeof(VitalCareAbpEntityFrameworkCoreModule).Assembly.GetName().Name);
                });

                // Activate Row-Level Security: sets app.user_id and app.user_role on each connection
                // ctx.DbContextOptions is DbContextOptionsBuilder<VitalCareAbpDbContext>
                ctx.DbContextOptions.AddInterceptors(
                    ctx.ServiceProvider.GetRequiredService<RlsSessionInterceptor>()
                );
            });
        });
    }
}
