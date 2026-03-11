using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using VitalCare.Abp.Entities;

namespace VitalCare.Abp;

public class VitalCareAbpDbContext : AbpDbContext<VitalCareAbpDbContext>
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<FirstLoginToken> FirstLoginTokens => Set<FirstLoginToken>();
    public DbSet<FailedLoginAttempt> FailedLoginAttempts => Set<FailedLoginAttempt>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<VitalReading> VitalReadings => Set<VitalReading>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<CaregiverLink> CaregiverLinks => Set<CaregiverLink>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public VitalCareAbpDbContext(DbContextOptions<VitalCareAbpDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.Role).HasMaxLength(32);
            e.Property(x => x.Name).HasMaxLength(512);
        });

        modelBuilder.Entity<Session>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.TokenHash);
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.ExpiresAt);
            e.Property(x => x.TokenHash).HasMaxLength(64);
            e.Property(x => x.CsrfTokenHash).HasMaxLength(64);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.TokenHash);
            e.HasIndex(x => new { x.UserId, x.ExpiresAt });
            e.Property(x => x.TokenHash).HasMaxLength(64);
        });

        modelBuilder.Entity<FirstLoginToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.TempTokenHash);
            e.HasIndex(x => x.ExpiresAt);
            e.Property(x => x.TempTokenHash).HasMaxLength(64);
        });

        modelBuilder.Entity<FailedLoginAttempt>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.Email, x.FailedAt });
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.IpAddress).HasMaxLength(64);
        });

        modelBuilder.Entity<PasswordResetToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.TokenHash);
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.ExpiresAt);
            e.Property(x => x.TokenHash).HasMaxLength(64);
        });

        modelBuilder.Entity<VitalReading>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.PatientId, x.RecordedAt });
            e.Property(x => x.Type).HasMaxLength(32);
            e.Property(x => x.Unit).HasMaxLength(16);
            e.Property(x => x.Source).HasMaxLength(16);
            e.HasOne(x => x.Patient).WithMany(u => u.VitalReadings).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Alert>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.PatientId, x.State });
            e.HasIndex(x => x.SlaDueAt);
            e.Property(x => x.VitalType).HasMaxLength(32);
            e.Property(x => x.Severity).HasMaxLength(32);
            e.Property(x => x.State).HasMaxLength(32);
            e.Property(x => x.Unit).HasMaxLength(16);
            e.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Rule).WithMany().HasForeignKey(x => x.RuleId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.AcknowledgedBy).WithMany(x => x.AlertsAcknowledged).HasForeignKey(x => x.AcknowledgedById).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.ResolvedBy).WithMany(x => x.AlertsResolved).HasForeignKey(x => x.ResolvedById).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AlertRule>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.VitalType, x.IsActive });
            e.Property(x => x.VitalType).HasMaxLength(32);
            e.Property(x => x.Severity).HasMaxLength(32);
            e.Property(x => x.Operator).HasMaxLength(16);
        });

        modelBuilder.Entity<CaregiverLink>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.PatientId, x.CaregiverId });
            e.HasOne(x => x.Patient).WithMany(x => x.CaregiverLinksAsPatient).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Caregiver).WithMany(x => x.CaregiverLinksAsCaregiver).HasForeignKey(x => x.CaregiverId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.Timestamp });
            e.HasIndex(x => x.Resource);
            e.HasIndex(x => x.PatientId);
            e.Property(x => x.Resource).HasMaxLength(64);
            e.Property(x => x.Action).HasMaxLength(32);
            e.Property(x => x.ResourceType).HasMaxLength(64);
            e.Property(x => x.DataType).HasMaxLength(64);
            e.Property(x => x.DataId).HasMaxLength(64);
            e.Property(x => x.AccessedFields).HasMaxLength(512);
            e.Property(x => x.UserEmail).HasMaxLength(256);
            e.Property(x => x.Role).HasMaxLength(32);
        });
    }
}
