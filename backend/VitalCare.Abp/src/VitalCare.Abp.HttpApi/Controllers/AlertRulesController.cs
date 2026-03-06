using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VitalCare.Abp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AlertRulesController : ControllerBase
{
    private readonly IAlertRuleAppService _alertRuleAppService;

    public AlertRulesController(IAlertRuleAppService alertRuleAppService)
    {
        _alertRuleAppService = alertRuleAppService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRules(CancellationToken cancellationToken)
    {
        var list = await _alertRuleAppService.GetListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRule(Guid id, CancellationToken cancellationToken)
    {
        var rule = await _alertRuleAppService.GetAsync(id, cancellationToken);
        if (rule == null) return NotFound();
        return Ok(rule);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRule([FromBody] CreateAlertRuleInput input, CancellationToken cancellationToken)
    {
        var rule = await _alertRuleAppService.CreateAsync(input, cancellationToken);
        return Ok(rule);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateRule(Guid id, [FromBody] UpdateAlertRuleInput input, CancellationToken cancellationToken)
    {
        var rule = await _alertRuleAppService.UpdateAsync(id, input, cancellationToken);
        if (rule == null) return NotFound();
        return Ok(rule);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRule(Guid id, CancellationToken cancellationToken)
    {
        await _alertRuleAppService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
