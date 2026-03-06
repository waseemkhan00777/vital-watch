using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VitalCare.Abp;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseAutofac();
await builder.AddApplicationAsync<VitalCareAbpHttpApiHostModule>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "VitalCareSecretKeyAtLeast32CharactersLong!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "VitalCare",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "VitalCare",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (ctx.Request.Cookies.TryGetValue("access_token", out var token))
                    ctx.Token = token;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();
await app.InitializeApplicationAsync();

if (app.Environment.IsDevelopment())
{
    await ApplyMigrationsAndSeedAsync(app);
}

await app.RunAsync();

static async Task ApplyMigrationsAndSeedAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<VitalCareAbpDbContext>();

    await dbContext.Database.MigrateAsync();

    // If migration history was out of sync (tables missing), EF Core often doesn't re-apply at runtime.
    // Drop history table so EnsureCreated() will create the schema, then record the initial migration.
    if (!await UsersTableExistsAsync(dbContext))
    {
        await dbContext.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS \"__EFMigrationsHistory\"");
        await dbContext.Database.EnsureCreatedAsync();
        // EnsureCreated() does not create __EFMigrationsHistory; create it and record the initial migration.
        await dbContext.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                ""MigrationId"" character varying(150) NOT NULL,
                ""ProductVersion"" character varying(32) NOT NULL,
                CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
            )");
        await dbContext.Database.ExecuteSqlRawAsync(
            "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('20250305120000_InitialPostgres', '8.0.11')");

        using (var seedScope = app.Services.CreateScope())
        {
            await VitalCareAbpDbSeeder.SeedAsync(seedScope.ServiceProvider);
        }
        return;
    }

    await VitalCareAbpDbSeeder.SeedAsync(scope.ServiceProvider);
}

static async Task<bool> UsersTableExistsAsync(VitalCareAbpDbContext dbContext)
{
    var conn = dbContext.Database.GetDbConnection();
    if (conn.State != System.Data.ConnectionState.Open)
        await conn.OpenAsync();
    await using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND LOWER(table_name) = 'users')";
        var exists = cmd.ExecuteScalar();
        return exists is true || (exists is long n && n != 0);
    }
}
