using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VitalCare.Abp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AlertsController : ControllerBase
{
    private readonly IAlertAppService _alertAppService;

    public AlertsController(IAlertAppService alertAppService)
    {
        _alertAppService = alertAppService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAlerts([FromQuery] Guid? patientId, CancellationToken cancellationToken)
    {
        var list = await _alertAppService.GetListAsync(patientId, cancellationToken);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAlert(Guid id, CancellationToken cancellationToken)
    {
        var alert = await _alertAppService.GetAsync(id, cancellationToken);
        if (alert == null) return NotFound();
        return Ok(alert);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateAlert(Guid id, [FromBody] UpdateAlertBody? body, CancellationToken cancellationToken)
    {
        var result = await _alertAppService.UpdateStateAsync(id, body?.State, body?.ClinicalNote, cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    public record UpdateAlertBody(string? State, string? ClinicalNote);
}
