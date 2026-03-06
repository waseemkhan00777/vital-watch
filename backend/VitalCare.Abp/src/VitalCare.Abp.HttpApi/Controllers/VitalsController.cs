using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VitalCare.Abp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VitalsController : ControllerBase
{
    private readonly IVitalReadingAppService _vitalReadingAppService;

    public VitalsController(IVitalReadingAppService vitalReadingAppService)
    {
        _vitalReadingAppService = vitalReadingAppService;
    }

    [HttpPost]
    public async Task<IActionResult> SubmitVital([FromBody] JsonElement body, CancellationToken cancellationToken)
    {
        var result = await _vitalReadingAppService.SubmitVitalAsync(body, cancellationToken);
        if (result == null)
            return BadRequest(new { message = "Invalid request or unauthorized." });
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetVitals([FromQuery] Guid? patientId, CancellationToken cancellationToken)
    {
        var list = await _vitalReadingAppService.GetListAsync(patientId, cancellationToken);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetVital(Guid id, CancellationToken cancellationToken)
    {
        var reading = await _vitalReadingAppService.GetAsync(id, cancellationToken);
        if (reading == null) return NotFound();
        return Ok(reading);
    }
}
