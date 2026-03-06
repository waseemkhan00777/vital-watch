using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VitalCare.Abp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IAuditAppService _auditAppService;

    public AuditController(IAuditAppService auditAppService)
    {
        _auditAppService = auditAppService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAudit(
        [FromQuery] Guid? userId,
        [FromQuery] string? resource,
        [FromQuery] string? resourceId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var list = await _auditAppService.GetListAsync(userId, resource, resourceId, from, to, cancellationToken);
        return Ok(list);
    }
}
