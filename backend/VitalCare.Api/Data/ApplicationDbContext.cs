using Microsoft.EntityFrameworkCore;
using VitalCare.Api.Data.Entities;

namespace VitalCare.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<VitalReading> VitalReadings => Set<VitalReading>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<CaregiverLink> CaregiverLinks => Set<CaregiverLink>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.Role).HasMaxLength(32);
            e.Property(x => x.Name).HasMaxLength(512); // Encrypted PHI can be longer
        });

        modelBuilder.Entity<VitalReading>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.PatientId, x.RecordedAt });
            e.Property(x => x.Type).HasMaxLength(32);
            e.Property(x => x.Unit).HasMaxLength(16);
            e.Property(x => x.Source).HasMaxLength(16);
            // Explicit inverse: User.VitalReadings. Stops EF from creating a second FK (UserId) by convention.
            e.HasOne(x => x.Patient).WithMany(u => u.VitalReadings).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.PatientId).HasColumnName("PatientId");
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
            e.Property(x => x.RuleId).HasColumnName("RuleId"); // DB has RuleId; prevent EF/Pomelo from using AlertRuleId
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
            e.Property(x => x.Resource).HasMaxLength(64);
            e.Property(x => x.Action).HasMaxLength(32);
            e.Property(x => x.UserEmail).HasMaxLength(256);
            e.Property(x => x.Role).HasMaxLength(32);
        });
    }
}
