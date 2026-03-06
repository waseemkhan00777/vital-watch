using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VitalCare.Abp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CaregiverLinksController : ControllerBase
{
    private readonly ICaregiverLinkAppService _caregiverLinkAppService;

    public CaregiverLinksController(ICaregiverLinkAppService caregiverLinkAppService)
    {
        _caregiverLinkAppService = caregiverLinkAppService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLinks(CancellationToken cancellationToken)
    {
        var list = await _caregiverLinkAppService.GetListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> CreateLink([FromBody] CreateLinkBody body, CancellationToken cancellationToken)
    {
        var link = await _caregiverLinkAppService.CreateAsync(body.PatientId, body.CaregiverId, cancellationToken);
        if (link == null) return BadRequest(new { message = "Invalid or duplicate link." });
        return Ok(link);
    }

    [HttpPost("{id:guid}/revoke")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken cancellationToken)
    {
        var ok = await _caregiverLinkAppService.RevokeAsync(id, cancellationToken);
        if (!ok) return NotFound();
        return Ok();
    }

    public record CreateLinkBody(Guid PatientId, Guid CaregiverId);
}
