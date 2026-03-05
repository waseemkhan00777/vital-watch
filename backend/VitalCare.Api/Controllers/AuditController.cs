using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitalCare.Api.Data;
using VitalCare.Api.DTOs;

namespace VitalCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AuditController(ApplicationDbContext db)
    {
        _db = db;
    }

    private string? UserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    private string? UserRole => User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetAuditLogs(
        [FromQuery] string? userId,
        [FromQuery] string? resource,
        [FromQuery] string? resourceId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int limit = 100,
        CancellationToken ct = default)
    {
        var query = _db.AuditLogs.AsNoTracking().AsQueryable();
        if (UserRole == "patient")
        {
            if (string.IsNullOrEmpty(UserId) || !Guid.TryParse(UserId, out var patientUid)) return Unauthorized();
            query = query.Where(a => a.UserId == patientUid);
        }
        else if (userId != null)
        {
            if (!Guid.TryParse(userId, out var uid)) return BadRequest(new { message = "Invalid userId." });
            query = query.Where(a => a.UserId == uid);
        }
        if (!string.IsNullOrEmpty(resource)) query = query.Where(a => a.Resource == resource);
        if (!string.IsNullOrEmpty(resourceId)) query = query.Where(a => a.ResourceId == resourceId);
        if (from.HasValue) query = query.Where(a => a.Timestamp >= from.Value);
        if (to.HasValue) query = query.Where(a => a.Timestamp <= to.Value);
        var list = await query.OrderByDescending(a => a.Timestamp).Take(limit).ToListAsync(ct);
        return Ok(list.Select(a => new AuditLogDto(a.Id.ToString(), a.UserId?.ToString(), a.UserEmail, a.Role, a.Resource, a.Action, a.ResourceId, a.Details, a.IpAddress, a.Timestamp.ToString("O"))));
    }
}
