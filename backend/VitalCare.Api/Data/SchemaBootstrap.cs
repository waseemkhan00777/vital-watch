using Microsoft.EntityFrameworkCore;

namespace VitalCare.Api.Data;

/// <summary>
/// Creates application tables when they are missing (e.g. only __EFMigrationsHistory exists
/// because EF did not discover the hand-written migration). Runs only in Development.
/// </summary>
public static class SchemaBootstrap
{
    public static async Task RunIfNeededAsync(ApplicationDbContext db, CancellationToken ct = default)
    {
        // Check if Users table exists without querying it (would throw if missing)
        var count = await db.Database.SqlQueryRaw<int>(
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'Users'")
            .ToListAsync(ct);
        var exists = count.FirstOrDefault() > 0;
        if (exists) return;

        var sql = GetInitialSchemaSql();
        foreach (var statement in SplitStatements(sql))
        {
            if (string.IsNullOrWhiteSpace(statement)) continue;
            try
            {
                await db.Database.ExecuteSqlRawAsync(statement, ct);
            }
            catch
            {
                // Ignore "table already exists" etc. on retries
            }
        }

        // Record migration so EF doesn't try to re-apply later if discovery is fixed
        await db.Database.ExecuteSqlRawAsync(
            "INSERT IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20250303000000_InitialCreate', '8.0.11')",
            ct);
    }

    private static string GetInitialSchemaSql()
    {
        return @"
CREATE TABLE IF NOT EXISTS `Users` (
    `Id` char(36) NOT NULL,
    `Email` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
    `PasswordHash` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Role` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `Name` varchar(512) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_Users` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE IF NOT EXISTS `AlertRules` (
    `Id` char(36) NOT NULL,
    `VitalType` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `Severity` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `Operator` varchar(16) CHARACTER SET utf8mb4 NOT NULL,
    `ThresholdMin` decimal(65,30) NULL,
    `ThresholdMax` decimal(65,30) NULL,
    `IsActive` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    CONSTRAINT `PK_AlertRules` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE IF NOT EXISTS `VitalReadings` (
    `Id` char(36) NOT NULL,
    `PatientId` char(36) NOT NULL,
    `Type` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `Value` decimal(65,30) NOT NULL,
    `ValueSecondary` decimal(65,30) NULL,
    `Unit` varchar(16) CHARACTER SET utf8mb4 NOT NULL,
    `RecordedAt` datetime(6) NOT NULL,
    `Source` varchar(16) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_VitalReadings` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_VitalReadings_Users_PatientId` FOREIGN KEY (`PatientId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE IF NOT EXISTS `Alerts` (
    `Id` char(36) NOT NULL,
    `PatientId` char(36) NOT NULL,
    `VitalType` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `Severity` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `State` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `Value` decimal(65,30) NOT NULL,
    `ValueSecondary` decimal(65,30) NULL,
    `Unit` varchar(16) CHARACTER SET utf8mb4 NOT NULL,
    `RuleId` char(36) NULL,
    `AcknowledgedAt` datetime(6) NULL,
    `AcknowledgedById` char(36) NULL,
    `EscalatedAt` datetime(6) NULL,
    `ResolvedAt` datetime(6) NULL,
    `ResolvedById` char(36) NULL,
    `SlaDueAt` datetime(6) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `ClinicalNote` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_Alerts` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Alerts_Users_AcknowledgedById` FOREIGN KEY (`AcknowledgedById`) REFERENCES `Users` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_Alerts_Users_PatientId` FOREIGN KEY (`PatientId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Alerts_Users_ResolvedById` FOREIGN KEY (`ResolvedById`) REFERENCES `Users` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_Alerts_AlertRules_RuleId` FOREIGN KEY (`RuleId`) REFERENCES `AlertRules` (`Id`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;

CREATE TABLE IF NOT EXISTS `CaregiverLinks` (
    `Id` char(36) NOT NULL,
    `PatientId` char(36) NOT NULL,
    `CaregiverId` char(36) NOT NULL,
    `ConsentedAt` datetime(6) NOT NULL,
    `RevokedAt` datetime(6) NULL,
    CONSTRAINT `PK_CaregiverLinks` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_CaregiverLinks_Users_CaregiverId` FOREIGN KEY (`CaregiverId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_CaregiverLinks_Users_PatientId` FOREIGN KEY (`PatientId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE IF NOT EXISTS `AuditLogs` (
    `Id` char(36) NOT NULL,
    `UserId` char(36) NULL,
    `UserEmail` varchar(256) CHARACTER SET utf8mb4 NULL,
    `Role` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `Resource` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `Action` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `ResourceId` longtext CHARACTER SET utf8mb4 NULL,
    `Details` longtext CHARACTER SET utf8mb4 NULL,
    `IpAddress` longtext CHARACTER SET utf8mb4 NULL,
    `Timestamp` datetime(6) NOT NULL,
    CONSTRAINT `PK_AuditLogs` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_Users_Email` ON `Users` (`Email`);
CREATE INDEX `IX_AlertRules_VitalType_IsActive` ON `AlertRules` (`VitalType`, `IsActive`);
CREATE INDEX `IX_VitalReadings_PatientId_RecordedAt` ON `VitalReadings` (`PatientId`, `RecordedAt`);
CREATE INDEX `IX_Alerts_AcknowledgedById` ON `Alerts` (`AcknowledgedById`);
CREATE INDEX `IX_Alerts_PatientId_State` ON `Alerts` (`PatientId`, `State`);
CREATE INDEX `IX_Alerts_ResolvedById` ON `Alerts` (`ResolvedById`);
CREATE INDEX `IX_Alerts_RuleId` ON `Alerts` (`RuleId`);
CREATE INDEX `IX_Alerts_SlaDueAt` ON `Alerts` (`SlaDueAt`);
CREATE INDEX `IX_CaregiverLinks_CaregiverId` ON `CaregiverLinks` (`CaregiverId`);
CREATE INDEX `IX_CaregiverLinks_PatientId_CaregiverId` ON `CaregiverLinks` (`PatientId`, `CaregiverId`);
CREATE INDEX `IX_AuditLogs_Resource` ON `AuditLogs` (`Resource`);
CREATE INDEX `IX_AuditLogs_UserId_Timestamp` ON `AuditLogs` (`UserId`, `Timestamp`);
";
    }

    private static IEnumerable<string> SplitStatements(string sql)
    {
        var statements = new List<string>();
        var current = new System.Text.StringBuilder();
        foreach (var line in sql.Split('\n'))
        {
            var t = line.Trim();
            if (t.StartsWith("CREATE ", StringComparison.OrdinalIgnoreCase) && current.Length > 0)
            {
                statements.Add(current.ToString().TrimEnd(' ', ';'));
                current.Clear();
            }
            current.AppendLine(line);
        }
        if (current.Length > 0)
            statements.Add(current.ToString().TrimEnd(' ', ';'));
        return statements;
    }
}
