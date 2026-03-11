using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VitalCare.Abp;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseAutofac();
await builder.AddApplicationAsync<VitalCareAbpHttpApiHostModule>();

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("JWT signing key is not configured. Set the Jwt:Key environment variable (e.g. JWT__Key).");
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
            },
            OnTokenValidated = async ctx =>
            {
                var token = ctx.Request.Headers.Authorization.FirstOrDefault()?.Split(' ', 2).LastOrDefault()
                    ?? (ctx.Request.Cookies.TryGetValue("access_token", out var c) ? c : null);
                if (string.IsNullOrEmpty(token)) return;
                var sessionsService = ctx.HttpContext.RequestServices.GetService<ISessionsService>();
                if (sessionsService == null) return;
                if (!await sessionsService.IsSessionValidAsync(token, ctx.HttpContext.RequestAborted))
                {
                    ctx.Fail(new UnauthorizedAccessException("Session invalid or expired."));
                    return;
                }
                _ = Task.Run(() => sessionsService.UpdateActivityAsync(token, default));
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Authentication endpoints: 5 attempts per 15 minutes (brute-force protection)
    options.AddFixedWindowLimiter("auth", config =>
    {
        config.Window = TimeSpan.FromMinutes(15);
        config.PermitLimit = 5;
        config.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 0;
    });

    // General API endpoints: 100 requests per minute
    options.AddFixedWindowLimiter("api", config =>
    {
        config.Window = TimeSpan.FromMinutes(1);
        config.PermitLimit = 100;
        config.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 0;
    });
});

var app = builder.Build();
await app.InitializeApplicationAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();

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
