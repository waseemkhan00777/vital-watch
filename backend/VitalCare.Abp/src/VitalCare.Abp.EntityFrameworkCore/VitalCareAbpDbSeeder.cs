using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp;

public static class VitalCareAbpDbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var db = serviceProvider.GetRequiredService<VitalCareAbpDbContext>();
        var userRepo = serviceProvider.GetRequiredService<IUserRepository>();
        var ruleRepo = serviceProvider.GetRequiredService<IAlertRuleRepository>();
        var encryption = serviceProvider.GetService<IEncryptionService>();
        var logger = serviceProvider.GetRequiredService<ILogger<VitalCareAbpDbContext>>();

        if (encryption == null)
        {
            logger.LogWarning("IEncryptionService not registered; seeding with plain names.");
            encryption = new PlainEncryptionService();
        }

        if (await userRepo.FindByEmailAsync("admin@vitalwatch.demo") != null)
        {
            logger.LogInformation("Seed already applied.");
            return;
        }

        var users = new[]
        {
            (Email: "admin@vitalwatch.demo", Password: "admin123", Name: "Admin", Role: "admin"),
            (Email: "nurse@vitalwatch.demo", Password: "nurse123", Name: "Nurse", Role: "clinician"),
            (Email: "patient@vitalwatch.demo", Password: "patient123", Name: "Patient", Role: "patient"),
            (Email: "caregiver@vitalwatch.demo", Password: "caregiver123", Name: "Caregiver", Role: "caregiver")
        };

        foreach (var u in users)
        {
            var user = new User(Guid.NewGuid())
            {
                Email = u.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(u.Password),
                Name = encryption.Encrypt(u.Name),
                Role = u.Role,
                CreatedAt = DateTime.UtcNow
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

    private sealed class PlainEncryptionService : IEncryptionService
    {
        public string Encrypt(string plainText) => plainText;
        public string Decrypt(string cipherText) => cipherText;
    }
}
