using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VitalCare.Abp.Controllers.Filters;

public class CsrfAuthorizationFilter : IAsyncAuthorizationFilter
{
    private static readonly HashSet<string> StateChangingMethods = new(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "PATCH", "DELETE" };
    private readonly ICsrfService _csrfService;

    public CsrfAuthorizationFilter(ICsrfService csrfService)
    {
        _csrfService = csrfService;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!StateChangingMethods.Contains(context.HttpContext.Request.Method))
            return;
        var endpoint = context.HttpContext.Features.Get<IEndpointFeature>()?.Endpoint;
        if (endpoint?.Metadata.GetMetadata<PublicAttribute>() != null)
            return;
        if (endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() != null)
            return;
        var token = context.HttpContext.Request.Headers.Authorization.FirstOrDefault()?.Split(' ', 2).LastOrDefault()
            ?? (context.HttpContext.Request.Cookies.TryGetValue("access_token", out var c) ? c : null);
        if (string.IsNullOrEmpty(token))
            return;
        var headerValue = context.HttpContext.Request.Headers["X-CSRF-Token"].FirstOrDefault();
        if (!await _csrfService.ValidateAsync(token, headerValue, context.HttpContext.RequestAborted))
        {
            context.Result = new ForbidResult();
        }
    }
}
