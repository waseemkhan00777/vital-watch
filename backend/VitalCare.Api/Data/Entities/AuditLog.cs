namespace VitalCare.Api.Data.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty; // e.g. vital_reading, alert, caregiver_link
    public string Action { get; set; } = string.Empty;   // e.g. create, read, update, delete
    public string? ResourceId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
