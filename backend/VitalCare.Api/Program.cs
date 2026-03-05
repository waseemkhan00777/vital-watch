using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VitalCare.Api.Data;
using VitalCare.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Port=3306;Database=VitalCare;User=root;Password=;";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// JWT — accept token from Authorization header or from httpOnly cookie (HIPAA: credentials in cookies)
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
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
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

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? new[] { "http://localhost:3000" })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<AlertEvaluationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Ensure DB exists, schema is created, and seed data in development
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (app.Environment.IsDevelopment())
    {
        try { await db.Database.MigrateAsync(); }
        catch { /* migrations may not be discovered or DB not ready */ }

        // When EF doesn't discover migrations, only __EFMigrationsHistory may exist.
        // Create application tables from SQL so the app can run.
        try { await VitalCare.Api.Data.SchemaBootstrap.RunIfNeededAsync(db); }
        catch { /* bootstrap may fail if DB not ready */ }

        try
        {
            var encryption = scope.ServiceProvider.GetService<VitalCare.Api.Services.IEncryptionService>();
            await VitalCare.Api.Data.DbSeeder.SeedAsync(db, encryption);
        }
        catch { /* seed may fail if DB not ready */ }
    }
}

app.Run();
