using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VitalCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class PingController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "ok", message = "pong", timestamp = DateTime.UtcNow });
    }
}
