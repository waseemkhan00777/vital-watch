using Volo.Abp.Modularity;

namespace VitalCare.Abp;

[DependsOn(typeof(VitalCareAbpApplicationContractsModule))]
public class VitalCareAbpHttpApiModule : AbpModule
{
}
