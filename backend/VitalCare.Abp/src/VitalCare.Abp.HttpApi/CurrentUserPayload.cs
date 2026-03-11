using System.Security.Claims;

namespace VitalCare.Abp.Controllers;

public record CurrentUserPayload(Guid Id, string Email, string Role, string Name)
{
    public static CurrentUserPayload? FromPrincipal(ClaimsPrincipal? principal)
    {
        if (principal?.Identity?.IsAuthenticated != true) return null;
        var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var id)) return null;
        var email = principal.FindFirst(ClaimTypes.Email)?.Value ?? principal.FindFirst("email")?.Value ?? "";
        var role = principal.FindFirst(ClaimTypes.Role)?.Value ?? principal.FindFirst("role")?.Value ?? "";
        var name = principal.FindFirst(ClaimTypes.Name)?.Value ?? principal.FindFirst("name")?.Value ?? "";
        return new CurrentUserPayload(id, email, role, name);
    }
}
