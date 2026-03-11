using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application;
using Volo.Abp.Modularity;
using VitalCare.Abp.Logging;
using VitalCare.Abp.Services;

namespace VitalCare.Abp;

[DependsOn(
    typeof(VitalCareAbpApplicationContractsModule),
    typeof(VitalCareAbpDomainModule),
    typeof(AbpDddApplicationModule)
)]
public class VitalCareAbpApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddScoped<IEncryptionService, EncryptionService>();
        context.Services.AddScoped<IPasswordHasher, PasswordHasher>();
        context.Services.AddScoped<ISessionsService, SessionsService>();
        context.Services.AddScoped<ICsrfService, CsrfService>();
        context.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        context.Services.AddScoped<IEmailSender, NoOpEmailSender>();
        context.Services.AddScoped<ILockoutService, LockoutService>();
        context.Services.AddScoped<IAccessControlService, AccessControlService>();
        context.Services.AddScoped<IAuditService, AuditService>();
        context.Services.AddScoped<AlertEvaluationService>();
        context.Services.AddScoped<IAuthAppService, AuthAppService>();
        context.Services.AddScoped<IUserAppService, UserAppService>();
        context.Services.AddScoped<IVitalReadingAppService, VitalReadingAppService>();
        context.Services.AddScoped<IAlertAppService, AlertAppService>();
        context.Services.AddScoped<IAlertRuleAppService, AlertRuleAppService>();
        context.Services.AddScoped<ICaregiverLinkAppService, CaregiverLinkAppService>();
        context.Services.AddScoped<IAuditAppService, AuditAppService>();
        context.Services.AddSingleton<IPhisSafeLoggerFactory, PhisSafeLoggerFactory>();
        context.Services.AddHostedService<TokenCleanupBackgroundService>();
    }
}
