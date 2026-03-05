using Microsoft.OpenApi.Models;
using Volo.Abp;
using Volo.Abp.Authentication.JwtBearer;
using Volo.Abp.Modularity;

namespace VitalCare.Abp;

[DependsOn(
    typeof(VitalCareAbpHttpApiModule),
    typeof(VitalCareAbpApplicationModule),
    typeof(VitalCareAbpEntityFrameworkCoreModule),
    typeof(AbpAuthenticationJwtBearerModule)
)]
public class VitalCareAbpHttpApiHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        ConfigureSwaggerServices(context);
    }

    private static void ConfigureSwaggerServices(ServiceConfigurationContext context)
    {
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

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "VitalCare API"));
        }

        app.UseCors(policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseConfiguredEndpoints();
    }
}
