using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitalCare.Api.Data;
using VitalCare.Api.Data.Entities;
using VitalCare.Api.DTOs;
using VitalCare.Api.Services;

namespace VitalCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VitalsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AlertEvaluationService _alertEval;
    private readonly IAuditService _audit;

    public VitalsController(ApplicationDbContext db, AlertEvaluationService alertEval, IAuditService audit)
    {
        _db = db;
        _alertEval = alertEval;
        _audit = audit;
    }

    private string? UserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    private string? UserRole => User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

    [HttpPost]
    public async Task<ActionResult<VitalReadingDto>> SubmitVital([FromBody] JsonElement body, CancellationToken ct)
    {
        if (UserRole != "patient" || string.IsNullOrEmpty(UserId) || !Guid.TryParse(UserId, out var patientGuid))
            return Forbid(); // Only patients submit their own; clinician submission could add patientId in body later.

        if (!body.TryGetProperty("type", out var typeEl))
            return BadRequest(new { message = "Missing 'type'." });

        var type = typeEl.GetString() ?? "";
        var recordedAt = body.TryGetProperty("recordedAt", out var ra) && ra.ValueKind == JsonValueKind.String
            ? DateTime.TryParse(ra.GetString(), out var dt) ? dt : DateTime.UtcNow
            : DateTime.UtcNow;

        (decimal value, decimal? valueSecondary, string unit) parsed;
        switch (type)
        {
            case "blood_pressure":
                if (!body.TryGetProperty("systolic", out var sys) || !body.TryGetProperty("diastolic", out var dia))
                    return BadRequest(new { message = "blood_pressure requires systolic and diastolic." });
                parsed = (sys.GetDecimal(), dia.GetDecimal(), "mmHg");
                break;
            case "heart_rate":
                if (!body.TryGetProperty("value", out var hr))
                    return BadRequest(new { message = "heart_rate requires value." });
                parsed = (hr.GetDecimal(), null, "bpm");
                break;
            case "blood_glucose":
                if (!body.TryGetProperty("value", out var bg))
                    return BadRequest(new { message = "blood_glucose requires value." });
                parsed = (bg.GetDecimal(), null, "mg/dL");
                break;
            case "oxygen_saturation":
                if (!body.TryGetProperty("value", out var spo2))
                    return BadRequest(new { message = "oxygen_saturation requires value." });
                parsed = (spo2.GetDecimal(), null, "%");
                break;
            case "weight":
                if (!body.TryGetProperty("value", out var w))
                    return BadRequest(new { message = "weight requires value." });
                parsed = (w.GetDecimal(), null, "kg");
                break;
            default:
                return BadRequest(new { message = "Invalid vital type." });
        }

        var reading = new VitalReading
        {
            Id = Guid.NewGuid(),
            PatientId = patientGuid,
            Type = type,
            Value = parsed.value,
            ValueSecondary = parsed.valueSecondary,
            Unit = parsed.unit,
            RecordedAt = recordedAt,
            Source = "manual",
            CreatedAt = DateTime.UtcNow
        };
        _db.VitalReadings.Add(reading);
        await _db.SaveChangesAsync(ct);
        await _alertEval.EvaluateAndCreateAlertsAsync(reading, ct);
        await _audit.LogAsync(UserId, null, UserRole ?? "", "vital_reading", "create", reading.Id.ToString(), ct: ct);

        return Ok(ToDto(reading));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VitalReadingDto>>> GetVitals(
        [FromQuery] string? patientId,
        [FromQuery] string? type,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int limit = 100,
        CancellationToken ct = default)
    {
        Guid? pid = null;
        if (patientId != null)
        {
            if (!Guid.TryParse(patientId, out var p)) return BadRequest(new { message = "Invalid patientId." });
            pid = p;
        }
        else if (UserRole == "patient")
        {
            if (string.IsNullOrEmpty(UserId) || !Guid.TryParse(UserId, out var uid)) return Unauthorized();
            pid = uid;
        }
        if (pid == null && UserRole != "admin" && UserRole != "clinician")
            return Forbid();

        var query = _db.VitalReadings.AsQueryable();
        if (pid != null) query = query.Where(r => r.PatientId == pid);
        if (!string.IsNullOrEmpty(type)) query = query.Where(r => r.Type == type);
        if (from.HasValue) query = query.Where(r => r.RecordedAt >= from.Value);
        if (to.HasValue) query = query.Where(r => r.RecordedAt <= to.Value);
        var list = await query.OrderByDescending(r => r.RecordedAt).Take(limit).ToListAsync(ct);

        // Caregiver: only if consented for this patient
        if (UserRole == "caregiver" && pid != null)
        {
            if (string.IsNullOrEmpty(UserId) || !Guid.TryParse(UserId, out var cgId)) return Unauthorized();
            var hasConsent = await _db.CaregiverLinks.AnyAsync(
                c => c.PatientId == pid && c.CaregiverId == cgId && c.RevokedAt == null, ct);
            if (!hasConsent) return Forbid();
        }

        return Ok(list.Select(ToDto));
    }

    private static VitalReadingDto ToDto(VitalReading r) => new(
        r.Id.ToString(),
        r.PatientId.ToString(),
        r.Type,
        r.Value,
        r.ValueSecondary,
        r.Unit,
        r.RecordedAt.ToString("O"),
        r.Source,
        r.CreatedAt.ToString("O")
    );
}
