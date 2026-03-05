using Volo.Abp.Modularity;

namespace VitalCare.Abp;

[DependsOn(
    typeof(VitalCareAbpApplicationContractsModule),
    typeof(VitalCareAbpApplicationModule)
)]
public class VitalCareAbpHttpApiModule : AbpModule
{
}
