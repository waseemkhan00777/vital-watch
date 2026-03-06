using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Castle;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;

namespace VitalCare.Abp;

[DependsOn(
    typeof(AbpAspNetCoreMvcModule),
    typeof(VitalCareAbpHttpApiModule),
    typeof(VitalCareAbpApplicationModule),
    typeof(VitalCareAbpEntityFrameworkCoreModule),
    typeof(AbpCastleCoreModule),
    typeof(AbpUnitOfWorkModule)
)]
public class VitalCareAbpHttpApiHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        context.Services.AddAbpSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "VitalCare API", Version = "v1" });
            options.DocInclusionPredicate((_, _) => true);
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();
        var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();

        app.UseRouting();

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "VitalCare API"));
        }

        app.UseCors(policy =>
        {
            policy.WithOrigins(configuration.GetSection("Cors:Origins").Get<string[]>() ?? new[] { "http://localhost:3000" })
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseConfiguredEndpoints();
    }
}
