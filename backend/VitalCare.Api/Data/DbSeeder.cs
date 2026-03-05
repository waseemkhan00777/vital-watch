using Microsoft.EntityFrameworkCore;
using VitalCare.Api.Data.Entities;
using VitalCare.Api.Services;

namespace VitalCare.Api.Data;

/// <summary>Seeds dummy data into all tables. Run once when DB is empty. Uses encryption for User names when provided.</summary>
public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, IEncryptionService? encryption = null, CancellationToken ct = default)
    {
        if (await db.Users.AnyAsync(ct)) return;

        string Enc(string name) => encryption?.Encrypt(name) ?? name;

        // --- Users ---
        var admin = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@vitalwatch.demo",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "admin",
            Name = Enc("Admin User"),
            CreatedAt = DateTime.UtcNow
        };
        var clinician = new User
        {
            Id = Guid.NewGuid(),
            Email = "nurse@vitalwatch.demo",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("nurse123"),
            Role = "clinician",
            Name = Enc("Jane Nurse"),
            CreatedAt = DateTime.UtcNow
        };
        var patient = new User
        {
            Id = Guid.NewGuid(),
            Email = "patient@vitalwatch.demo",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("patient123"),
            Role = "patient",
            Name = Enc("John Patient"),
            CreatedAt = DateTime.UtcNow
        };
        var caregiver = new User
        {
            Id = Guid.NewGuid(),
            Email = "caregiver@vitalwatch.demo",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("caregiver123"),
            Role = "caregiver",
            Name = Enc("Care Giver"),
            CreatedAt = DateTime.UtcNow
        };
        db.Users.AddRange(admin, clinician, patient, caregiver);
        await db.SaveChangesAsync(ct);

        // --- Alert rules ---
        var ruleBpCritical = new AlertRule { Id = Guid.NewGuid(), VitalType = "blood_pressure", Severity = "critical", Operator = "above", ThresholdMin = 180, ThresholdMax = null, IsActive = true, CreatedAt = DateTime.UtcNow };
        var ruleBpHigh = new AlertRule { Id = Guid.NewGuid(), VitalType = "blood_pressure", Severity = "high", Operator = "above", ThresholdMin = 140, ThresholdMax = null, IsActive = true, CreatedAt = DateTime.UtcNow };
        var ruleHrCritical = new AlertRule { Id = Guid.NewGuid(), VitalType = "heart_rate", Severity = "critical", Operator = "above", ThresholdMin = 120, ThresholdMax = null, IsActive = true, CreatedAt = DateTime.UtcNow };
        var ruleHrHigh = new AlertRule { Id = Guid.NewGuid(), VitalType = "heart_rate", Severity = "high", Operator = "above", ThresholdMin = 100, ThresholdMax = null, IsActive = true, CreatedAt = DateTime.UtcNow };
        var ruleSpo2 = new AlertRule { Id = Guid.NewGuid(), VitalType = "oxygen_saturation", Severity = "critical", Operator = "below", ThresholdMin = null, ThresholdMax = 90, IsActive = true, CreatedAt = DateTime.UtcNow };
        var ruleGlucose = new AlertRule { Id = Guid.NewGuid(), VitalType = "blood_glucose", Severity = "critical", Operator = "above", ThresholdMin = 400, ThresholdMax = null, IsActive = true, CreatedAt = DateTime.UtcNow };
        db.AlertRules.AddRange(ruleBpCritical, ruleBpHigh, ruleHrCritical, ruleHrHigh, ruleSpo2, ruleGlucose);
        await db.SaveChangesAsync(ct);

        // --- Vital readings (patient) ---
        var baseTime = DateTime.UtcNow.AddDays(-2);
        var v1 = new VitalReading { Id = Guid.NewGuid(), PatientId = patient.Id, Type = "blood_pressure", Value = 128, ValueSecondary = 82, Unit = "mmHg", RecordedAt = baseTime, Source = "manual", CreatedAt = baseTime };
        var v2 = new VitalReading { Id = Guid.NewGuid(), PatientId = patient.Id, Type = "blood_pressure", Value = 185, ValueSecondary = 98, Unit = "mmHg", RecordedAt = baseTime.AddHours(2), Source = "manual", CreatedAt = baseTime.AddHours(2) };
        var v3 = new VitalReading { Id = Guid.NewGuid(), PatientId = patient.Id, Type = "heart_rate", Value = 72, Unit = "bpm", RecordedAt = baseTime.AddHours(4), Source = "manual", CreatedAt = baseTime.AddHours(4) };
        var v4 = new VitalReading { Id = Guid.NewGuid(), PatientId = patient.Id, Type = "blood_glucose", Value = 108, Unit = "mg/dL", RecordedAt = baseTime.AddHours(6), Source = "manual", CreatedAt = baseTime.AddHours(6) };
        var v5 = new VitalReading { Id = Guid.NewGuid(), PatientId = patient.Id, Type = "oxygen_saturation", Value = 97, Unit = "%", RecordedAt = baseTime.AddHours(8), Source = "manual", CreatedAt = baseTime.AddHours(8) };
        var v6 = new VitalReading { Id = Guid.NewGuid(), PatientId = patient.Id, Type = "weight", Value = 72.5m, Unit = "kg", RecordedAt = baseTime.AddHours(10), Source = "manual", CreatedAt = baseTime.AddHours(10) };
        db.VitalReadings.AddRange(v1, v2, v3, v4, v5, v6);
        await db.SaveChangesAsync(ct);

        // --- Alerts (from high BP reading v2; one flagged, one acknowledged, one resolved) ---
        var slaDue = baseTime.AddHours(2).AddHours(1); // critical 1h
        var alert1 = new Alert
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            VitalType = "blood_pressure",
            Severity = "critical",
            State = "flagged",
            Value = 185,
            ValueSecondary = 98,
            Unit = "mmHg",
            RuleId = ruleBpCritical.Id,
            SlaDueAt = slaDue,
            CreatedAt = baseTime.AddHours(2)
        };
        var alert2 = new Alert
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            VitalType = "blood_pressure",
            Severity = "high",
            State = "acknowledged",
            Value = 142,
            ValueSecondary = 90,
            Unit = "mmHg",
            RuleId = ruleBpHigh.Id,
            AcknowledgedAt = baseTime.AddHours(3),
            AcknowledgedById = clinician.Id,
            SlaDueAt = baseTime.AddHours(6),
            CreatedAt = baseTime.AddHours(2).AddMinutes(30)
        };
        var alert3 = new Alert
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            VitalType = "heart_rate",
            Severity = "high",
            State = "resolved",
            Value = 105,
            Unit = "bpm",
            RuleId = ruleHrHigh.Id,
            AcknowledgedAt = baseTime.AddHours(5),
            AcknowledgedById = clinician.Id,
            ResolvedAt = baseTime.AddHours(5).AddMinutes(15),
            ResolvedById = clinician.Id,
            ClinicalNote = "Patient advised to rest; recheck in 1h",
            SlaDueAt = baseTime.AddHours(6),
            CreatedAt = baseTime.AddHours(4).AddMinutes(30)
        };
        db.Alerts.AddRange(alert1, alert2, alert3);
        await db.SaveChangesAsync(ct);

        // --- Caregiver link (patient consented caregiver) ---
        var link = new CaregiverLink
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            CaregiverId = caregiver.Id,
            ConsentedAt = baseTime.AddDays(-1)
        };
        db.CaregiverLinks.Add(link);
        await db.SaveChangesAsync(ct);

        // --- Audit logs ---
        var auditEntries = new[]
        {
            new AuditLog { Id = Guid.NewGuid(), UserId = patient.Id, UserEmail = patient.Email, Role = "patient", Resource = "auth", Action = "login", IpAddress = "127.0.0.1", Timestamp = baseTime },
            new AuditLog { Id = Guid.NewGuid(), UserId = patient.Id, UserEmail = patient.Email, Role = "patient", Resource = "vital_reading", Action = "create", ResourceId = v1.Id.ToString(), Timestamp = baseTime },
            new AuditLog { Id = Guid.NewGuid(), UserId = patient.Id, UserEmail = patient.Email, Role = "patient", Resource = "vital_reading", Action = "create", ResourceId = v2.Id.ToString(), Timestamp = baseTime.AddHours(2) },
            new AuditLog { Id = Guid.NewGuid(), UserId = clinician.Id, UserEmail = clinician.Email, Role = "clinician", Resource = "alert", Action = "update", ResourceId = alert2.Id.ToString(), Details = "acknowledged", Timestamp = baseTime.AddHours(3) },
            new AuditLog { Id = Guid.NewGuid(), UserId = clinician.Id, UserEmail = clinician.Email, Role = "clinician", Resource = "alert", Action = "update", ResourceId = alert3.Id.ToString(), Details = "resolved", Timestamp = baseTime.AddHours(5).AddMinutes(15) },
            new AuditLog { Id = Guid.NewGuid(), UserId = patient.Id, UserEmail = patient.Email, Role = "patient", Resource = "caregiver_link", Action = "create", ResourceId = link.Id.ToString(), Timestamp = baseTime.AddDays(-1) },
            new AuditLog { Id = Guid.NewGuid(), UserId = admin.Id, UserEmail = admin.Email, Role = "admin", Resource = "auth", Action = "login", IpAddress = "127.0.0.1", Timestamp = baseTime.AddDays(-2) }
        };
        db.AuditLogs.AddRange(auditEntries);
        await db.SaveChangesAsync(ct);
    }
}
