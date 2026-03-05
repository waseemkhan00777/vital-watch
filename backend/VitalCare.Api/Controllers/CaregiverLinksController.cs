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
public class CaregiverLinksController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditService _audit;
    private readonly IEncryptionService _encryption;

    public CaregiverLinksController(ApplicationDbContext db, IAuditService audit, IEncryptionService encryption)
    {
        _db = db;
        _audit = audit;
        _encryption = encryption;
    }

    private string? UserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    private string? UserRole => User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CaregiverLinkDto>>> GetLinks(
        [FromQuery] string? patientId,
        [FromQuery] string? caregiverId,
        [FromQuery] bool? activeOnly,
        CancellationToken ct = default)
    {
        var query = _db.CaregiverLinks.Include(c => c.Caregiver).Include(c => c.Patient).AsQueryable();
        if (UserRole == "patient")
        {
            if (string.IsNullOrEmpty(UserId) || !Guid.TryParse(UserId, out var uid)) return Unauthorized();
            query = query.Where(c => c.PatientId == uid);
        }
        else if (UserRole == "caregiver")
        {
            if (string.IsNullOrEmpty(UserId) || !Guid.TryParse(UserId, out var cgId)) return Unauthorized();
            query = query.Where(c => c.CaregiverId == cgId);
        }
        else if (patientId != null)
        {
            if (!Guid.TryParse(patientId, out var pid)) return BadRequest(new { message = "Invalid patientId." });
            query = query.Where(c => c.PatientId == pid);
        }
        if (caregiverId != null)
        {
            if (!Guid.TryParse(caregiverId, out var cid)) return BadRequest(new { message = "Invalid caregiverId." });
            query = query.Where(c => c.CaregiverId == cid);
        }
        if (activeOnly == true) query = query.Where(c => c.RevokedAt == null);
        var list = await query.OrderByDescending(c => c.ConsentedAt).ToListAsync(ct);
        return Ok(list.Select(c => ToDto(c, c.Caregiver != null ? _encryption.Decrypt(c.Caregiver.Name) : null)));
    }

    [HttpPost]
    [Authorize(Roles = "patient,admin")]
    public async Task<ActionResult<CaregiverLinkDto>> InviteCaregiver([FromBody] InviteCaregiverRequest req, CancellationToken ct)
    {
        Guid? patientId = null;
        if (UserRole == "admin")
        {
            if (!string.IsNullOrEmpty(req.PatientId) && !Guid.TryParse(req.PatientId, out var pid)) return BadRequest(new { message = "Invalid PatientId." });
            if (!string.IsNullOrEmpty(req.PatientId)) patientId = Guid.Parse(req.PatientId);
        }
        else
        {
            if (string.IsNullOrEmpty(UserId) || !Guid.TryParse(UserId, out var uid)) return Unauthorized();
            patientId = uid;
        }
        if (patientId == null) return BadRequest(new { message = "PatientId required when inviting as admin." });
        var caregiver = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.CaregiverEmail && u.Role == "caregiver", ct);
        if (caregiver == null) return NotFound(new { message = "Caregiver not found with that email." });

        var existing = await _db.CaregiverLinks.FirstOrDefaultAsync(
            c => c.PatientId == patientId && c.CaregiverId == caregiver.Id && c.RevokedAt == null, ct);
        if (existing != null) return BadRequest(new { message = "Link already exists." });

        var link = new CaregiverLink
        {
            Id = Guid.NewGuid(),
            PatientId = patientId.Value,
            CaregiverId = caregiver.Id,
            ConsentedAt = DateTime.UtcNow
        };
        _db.CaregiverLinks.Add(link);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(UserId, null, UserRole ?? "", "caregiver_link", "create", link.Id.ToString(), ct: ct);
        return Ok(ToDto(link, _encryption.Decrypt(caregiver.Name)));
    }

    [HttpPost("{id}/revoke")]
    [Authorize(Roles = "patient,admin,caregiver")]
    public async Task<ActionResult> Revoke(Guid id, CancellationToken ct)
    {
        var link = await _db.CaregiverLinks.Include(c => c.Caregiver).FirstOrDefaultAsync(c => c.Id == id, ct);
        if (link == null) return NotFound();
        if (UserRole == "caregiver" && (string.IsNullOrEmpty(UserId) || !Guid.TryParse(UserId, out var cgId) || link.CaregiverId != cgId)) return Forbid();
        if (UserRole == "patient" && (string.IsNullOrEmpty(UserId) || !Guid.TryParse(UserId, out var pId) || link.PatientId != pId)) return Forbid();

        link.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(UserId, null, UserRole ?? "", "caregiver_link", "revoke", id.ToString(), ct: ct);
        return NoContent();
    }

    private static CaregiverLinkDto ToDto(CaregiverLink c, string? caregiverName = null) => new(
        c.Id.ToString(),
        c.PatientId.ToString(),
        c.CaregiverId.ToString(),
        caregiverName ?? "",
        c.ConsentedAt.ToString("O"),
        c.RevokedAt?.ToString("O")
    );
}
