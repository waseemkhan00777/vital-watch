using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitalCare.Api.Data;
using VitalCare.Api.DTOs;
using VitalCare.Api.Services;

namespace VitalCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AlertsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditService _audit;
    private readonly IEncryptionService _encryption;

    public AlertsController(ApplicationDbContext db, IAuditService audit, IEncryptionService encryption)
    {
        _db = db;
        _audit = audit;
        _encryption = encryption;
    }

    private string? UserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    private string? UserRole => User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AlertDto>>> GetAlerts(
        [FromQuery] string? patientId,
        [FromQuery] string? state,
        [FromQuery] string? severity,
        [FromQuery] int limit = 100,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(UserId)) return Unauthorized();
        var query = _db.Alerts.Include(a => a.Patient).Include(a => a.AcknowledgedBy).Include(a => a.ResolvedBy).AsQueryable();

        if (UserRole == "patient")
        {
            if (!Guid.TryParse(UserId, out var uid)) return Unauthorized();
            query = query.Where(a => a.PatientId == uid);
        }
        else if (UserRole == "caregiver")
        {
            if (!Guid.TryParse(UserId, out var caregiverId)) return Unauthorized();
            var consentedPatientIds = await _db.CaregiverLinks
                .Where(c => c.CaregiverId == caregiverId && c.RevokedAt == null)
                .Select(c => c.PatientId)
                .ToListAsync(ct);
            query = query.Where(a => consentedPatientIds.Contains(a.PatientId));
        }
        else if (patientId != null)
        {
            if (!Guid.TryParse(patientId, out var pid)) return BadRequest(new { message = "Invalid patientId." });
            query = query.Where(a => a.PatientId == pid);
        }

        if (!string.IsNullOrEmpty(state)) query = query.Where(a => a.State == state);
        if (!string.IsNullOrEmpty(severity)) query = query.Where(a => a.Severity == severity);
        var list = await query.OrderByDescending(a => a.CreatedAt).Take(limit).ToListAsync(ct);
        return Ok(list.Select(ToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AlertDto>> GetAlert(Guid id, CancellationToken ct)
    {
        var alert = await _db.Alerts.Include(a => a.Patient).Include(a => a.AcknowledgedBy).Include(a => a.ResolvedBy)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
        if (alert == null) return NotFound();
        if (UserRole == "patient")
        {
            if (string.IsNullOrEmpty(UserId) || !Guid.TryParse(UserId, out var uid)) return Unauthorized();
            if (alert.PatientId != uid) return Forbid();
        }
        else if (UserRole == "caregiver")
        {
            if (string.IsNullOrEmpty(UserId) || !Guid.TryParse(UserId, out var cgId)) return Unauthorized();
            var hasConsent = await _db.CaregiverLinks.AnyAsync(c => c.PatientId == alert.PatientId && c.CaregiverId == cgId && c.RevokedAt == null, ct);
            if (!hasConsent) return Forbid();
        }
        return Ok(ToDto(alert));
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "admin,clinician")]
    public async Task<ActionResult<AlertDto>> UpdateAlert(Guid id, [FromBody] UpdateAlertRequest req, CancellationToken ct)
    {
        var alert = await _db.Alerts.Include(a => a.Patient).Include(a => a.AcknowledgedBy).Include(a => a.ResolvedBy)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
        if (alert == null) return NotFound();

        if (req.State != null)
        {
            alert.State = req.State;
            if (req.State == "acknowledged" && Guid.TryParse(UserId, out var ackBy))
            {
                alert.AcknowledgedAt = DateTime.UtcNow;
                alert.AcknowledgedById = ackBy;
            }
            else if (req.State == "resolved" && Guid.TryParse(UserId, out var resBy))
            {
                alert.ResolvedAt = DateTime.UtcNow;
                alert.ResolvedById = resBy;
            }
        }
        if (req.ClinicalNote != null) alert.ClinicalNote = req.ClinicalNote;
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(UserId, null, UserRole ?? "", "alert", "update", id.ToString(), req.ClinicalNote, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        return Ok(ToDto(alert));
    }

    private AlertDto ToDto(Data.Entities.Alert a) => new(
        a.Id.ToString(),
        a.PatientId.ToString(),
        a.Patient != null ? _encryption.Decrypt(a.Patient.Name) : null,
        a.VitalType,
        a.Severity,
        a.State,
        a.Value,
        a.ValueSecondary,
        a.Unit,
        a.RuleId?.ToString(),
        a.AcknowledgedAt?.ToString("O"),
        a.AcknowledgedBy != null ? _encryption.Decrypt(a.AcknowledgedBy.Name) : null,
        a.EscalatedAt?.ToString("O"),
        a.ResolvedAt?.ToString("O"),
        a.ResolvedBy != null ? _encryption.Decrypt(a.ResolvedBy.Name) : null,
        a.SlaDueAt.ToString("O"),
        a.CreatedAt.ToString("O"),
        a.ClinicalNote
    );
}
