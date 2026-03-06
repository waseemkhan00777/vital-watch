using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VitalCare.Abp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserAppService _userAppService;

    public UsersController(IUserAppService userAppService)
    {
        _userAppService = userAppService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var list = await _userAppService.GetListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userAppService.GetAsync(id, cancellationToken);
        if (user == null) return NotFound();
        return Ok(user);
    }
}
