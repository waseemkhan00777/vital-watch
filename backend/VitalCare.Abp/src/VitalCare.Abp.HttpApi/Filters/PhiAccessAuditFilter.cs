using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VitalCare.Abp.Controllers.Filters;

/// <summary>
/// Global action filter that automatically writes an audit log entry after any successful
/// response from an action decorated with [PhiResource(...)].
/// Satisfies HIPAA § 164.312(b): audit controls must record and examine ePHI access.
/// Register as a global filter in VitalCareAbpHttpApiHostModule.
/// </summary>
public class PhiAccessAuditFilter(IAuditService auditService) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var result = await next();

        // Only log successful responses; skip on exception or cancelled requests
        if (result.Exception != null || context.HttpContext.RequestAborted.IsCancellationRequested)
            return;

        var endpoint = context.HttpContext.Features.Get<IEndpointFeature>()?.Endpoint;

        // Action-level attribute takes precedence; fall back to controller-level
        var attr = endpoint?.Metadata.GetMetadata<PhiResourceAttribute>();
        if (attr == null) return;

        var user = CurrentUserPayload.FromPrincipal(context.HttpContext.User);
        if (user == null) return;

        // Extract patientId from route/query arguments if present
        var patientId = context.ActionArguments.TryGetValue("patientId", out var pid)
            ? pid?.ToString()
            : context.HttpContext.Request.RouteValues["patientId"]?.ToString();

        Guid? patientGuid = Guid.TryParse(patientId, out var g) ? g : null;
        var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString();

        await auditService.LogPHIAccessAsync(
            user.Id,
            user.Email,
            user.Role,
            attr.Resource,
            patientId ?? "",
            patientGuid,
            null,
            context.HttpContext.RequestAborted);
    }
}
