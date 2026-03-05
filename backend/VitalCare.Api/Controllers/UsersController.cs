using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitalCare.Api.Data;
using VitalCare.Api.DTOs;
using VitalCare.Api.Services;

namespace VitalCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IEncryptionService _encryption;

    public UsersController(ApplicationDbContext db, IEncryptionService encryption)
    {
        _db = db;
        _encryption = encryption;
    }

    private string? UserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    private string? UserRole => User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

    [HttpGet]
    [Authorize(Roles = "admin,clinician")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(
        [FromQuery] string? role,
        [FromQuery] string? search,
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
    {
        var query = _db.Users.AsNoTracking().Where(u => true);
        if (!string.IsNullOrEmpty(role)) query = query.Where(u => u.Role == role);
        if (!string.IsNullOrEmpty(search))
            query = query.Where(u => u.Email.Contains(search));
        var list = await query.OrderBy(u => u.Email).Take(limit).ToListAsync(ct);
        return Ok(list.Select(u => new UserDto(u.Id.ToString(), u.Email, u.Role, _encryption.Decrypt(u.Name))));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id, CancellationToken ct)
    {
        if (UserRole != "admin" && UserRole != "clinician" && id.ToString() != UserId)
            return Forbid();
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user == null) return NotFound();
        return Ok(new UserDto(user.Id.ToString(), user.Email, user.Role, _encryption.Decrypt(user.Name)));
    }
}
