using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp;

public static class VitalCareAbpDbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var env = serviceProvider.GetRequiredService<IHostEnvironment>();
        var logger = serviceProvider.GetRequiredService<ILogger<VitalCareAbpDbContext>>();

        if (!env.IsDevelopment())
        {
            logger.LogInformation("Skipping database seeding: not running in Development environment.");
            return;
        }

        var db = serviceProvider.GetRequiredService<VitalCareAbpDbContext>();
        var userRepo = serviceProvider.GetRequiredService<IUserRepository>();
        var ruleRepo = serviceProvider.GetRequiredService<IAlertRuleRepository>();
        var encryption = serviceProvider.GetRequiredService<IEncryptionService>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        if (await userRepo.FindByEmailAsync("admin@vitalwatch.demo") != null)
        {
            logger.LogInformation("Seed already applied.");
            return;
        }

        // Passwords are read from configuration (user-secrets or environment variables).
        // Set via: dotnet user-secrets set "Seed:AdminPassword" "<strong-password>"
        var adminPwd = configuration["Seed:AdminPassword"]
            ?? throw new InvalidOperationException("Seed:AdminPassword is not configured. Set it via user-secrets or environment variable Seed__AdminPassword.");
        var clinicianPwd = configuration["Seed:ClinicianPassword"]
            ?? throw new InvalidOperationException("Seed:ClinicianPassword is not configured.");
        var patientPwd = configuration["Seed:PatientPassword"]
            ?? throw new InvalidOperationException("Seed:PatientPassword is not configured.");
        var caregiverPwd = configuration["Seed:CaregiverPassword"]
            ?? throw new InvalidOperationException("Seed:CaregiverPassword is not configured.");

        var users = new[]
        {
            (Email: "admin@vitalwatch.demo", Password: adminPwd, Name: "Admin", Role: "admin"),
            (Email: "nurse@vitalwatch.demo", Password: clinicianPwd, Name: "Nurse", Role: "clinician"),
            (Email: "patient@vitalwatch.demo", Password: patientPwd, Name: "Patient", Role: "patient"),
            (Email: "caregiver@vitalwatch.demo", Password: caregiverPwd, Name: "Caregiver", Role: "caregiver")
        };

        foreach (var u in users)
        {
            var user = new User(Guid.NewGuid())
            {
                Email = u.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(u.Password),
                Name = encryption.Encrypt(u.Name),
                Role = u.Role,
                CreatedAt = DateTime.UtcNow,
                // Require a password change on first login for all seeded accounts
                MustChangePassword = true
            };
            await userRepo.InsertAsync(user);
        }

        var rules = new[]
        {
            new { VitalType = "blood_pressure", Severity = "critical", Operator = "above", Min = (decimal?)180m, Max = (decimal?)null },
            new { VitalType = "blood_pressure", Severity = "high", Operator = "above", Min = (decimal?)160m, Max = (decimal?)null },
            new { VitalType = "heart_rate", Severity = "critical", Operator = "above", Min = (decimal?)120m, Max = (decimal?)null },
            new { VitalType = "heart_rate", Severity = "high", Operator = "below", Min = (decimal?)null, Max = (decimal?)50m },
            new { VitalType = "blood_glucose", Severity = "critical", Operator = "above", Min = (decimal?)400m, Max = (decimal?)null },
            new { VitalType = "oxygen_saturation", Severity = "critical", Operator = "below", Min = (decimal?)null, Max = (decimal?)90m }
        };

        foreach (var r in rules)
        {
            var rule = new AlertRule(Guid.NewGuid())
            {
                VitalType = r.VitalType,
                Severity = r.Severity,
                Operator = r.Operator,
                ThresholdMin = r.Min,
                ThresholdMax = r.Max,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await ruleRepo.InsertAsync(rule);
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Seed completed.");
    }
}
