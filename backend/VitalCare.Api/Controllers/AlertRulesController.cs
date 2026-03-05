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
[Authorize(Roles = "admin")]
public class AlertRulesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditService _audit;

    public AlertRulesController(ApplicationDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    private string? UserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AlertRuleDto>>> GetRules([FromQuery] string? vitalType, [FromQuery] bool? activeOnly, CancellationToken ct = default)
    {
        var query = _db.AlertRules.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(vitalType)) query = query.Where(r => r.VitalType == vitalType);
        if (activeOnly == true) query = query.Where(r => r.IsActive);
        var list = await query.ToListAsync(ct);
        return Ok(list.Select(ToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AlertRuleDto>> GetRule(Guid id, CancellationToken ct)
    {
        var rule = await _db.AlertRules.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct);
        if (rule == null) return NotFound();
        return Ok(ToDto(rule));
    }

    [HttpPost]
    public async Task<ActionResult<AlertRuleDto>> CreateRule([FromBody] AlertRuleRequest req, CancellationToken ct)
    {
        var rule = new AlertRule
        {
            Id = Guid.NewGuid(),
            VitalType = req.VitalType,
            Severity = req.Severity,
            Operator = req.Operator,
            ThresholdMin = req.ThresholdMin,
            ThresholdMax = req.ThresholdMax,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _db.AlertRules.Add(rule);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(UserId, null, "admin", "alert_rule", "create", rule.Id.ToString(), ct: ct);
        return Ok(ToDto(rule));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AlertRuleDto>> UpdateRule(Guid id, [FromBody] AlertRuleRequest req, CancellationToken ct)
    {
        var rule = await _db.AlertRules.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (rule == null) return NotFound();
        rule.VitalType = req.VitalType;
        rule.Severity = req.Severity;
        rule.Operator = req.Operator;
        rule.ThresholdMin = req.ThresholdMin;
        rule.ThresholdMax = req.ThresholdMax;
        rule.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(UserId, null, "admin", "alert_rule", "update", id.ToString(), ct: ct);
        return Ok(ToDto(rule));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRule(Guid id, CancellationToken ct)
    {
        var rule = await _db.AlertRules.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (rule == null) return NotFound();
        rule.IsActive = false;
        rule.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(UserId, null, "admin", "alert_rule", "delete", id.ToString(), ct: ct);
        return NoContent();
    }

    private static AlertRuleDto ToDto(AlertRule r) => new(
        r.Id.ToString(),
        r.VitalType,
        r.Severity,
        r.Operator,
        r.ThresholdMin,
        r.ThresholdMax,
        r.IsActive
    );
}
