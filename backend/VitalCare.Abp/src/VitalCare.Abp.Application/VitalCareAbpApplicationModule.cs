using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application;
using Volo.Abp.Modularity;
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
        context.Services.AddScoped<IAuditService, AuditService>();
        context.Services.AddScoped<AlertEvaluationService>();
        context.Services.AddScoped<IAuthAppService, AuthAppService>();
        context.Services.AddScoped<IUserAppService, UserAppService>();
        context.Services.AddScoped<IVitalReadingAppService, VitalReadingAppService>();
        context.Services.AddScoped<IAlertAppService, AlertAppService>();
        context.Services.AddScoped<IAlertRuleAppService, AlertRuleAppService>();
        context.Services.AddScoped<ICaregiverLinkAppService, CaregiverLinkAppService>();
        context.Services.AddScoped<IAuditAppService, AuditAppService>();
    }
}
