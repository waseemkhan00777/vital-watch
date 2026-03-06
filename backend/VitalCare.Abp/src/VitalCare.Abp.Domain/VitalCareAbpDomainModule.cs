using Volo.Abp.Modularity;

namespace VitalCare.Abp;

[DependsOn(typeof(VitalCareAbpDomainSharedModule))]
public class VitalCareAbpDomainModule : AbpModule
{
}
