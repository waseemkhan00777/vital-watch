using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VitalCare.Abp.Controllers.Filters;

public class RolesAuthorizationFilter(IAuditService auditService) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var endpoint = context.HttpContext.Features.Get<IEndpointFeature>()?.Endpoint;
        var rolesAttr = endpoint?.Metadata.GetMetadata<RolesAttribute>();
        if (rolesAttr == null) return;

        if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var role = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
            ?? context.HttpContext.User.FindFirst("role")?.Value;

        if (!rolesAttr.IsAllowed(role))
        {
            context.Result = new ForbidResult();

            var user = CurrentUserPayload.FromPrincipal(context.HttpContext.User);
            var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            var resource = context.HttpContext.Request.Path.ToString();
            await auditService.LogAccessDenialAsync(user?.Id, role, resource, ip, context.HttpContext.RequestAborted);
        }
    }
}
