using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VitalCare.Abp.Controllers.Filters;

/// <summary>
/// Global exception filter that maps exceptions to HTTP status and returns sanitized responses
/// so that PHI, emails, UUIDs, stack traces, and internal details are never leaked.
/// </summary>
public class HttpExceptionFilter : IExceptionFilter
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public void OnException(ExceptionContext context)
    {
        if (context.ExceptionHandled) return;

        var (statusCode, safeMessage) = MapException(context.Exception);
        var response = new
        {
            error = safeMessage,
            status = (int)statusCode
        };

        context.Result = new JsonResult(response)
        {
            StatusCode = (int)statusCode
        };
        context.ExceptionHandled = true;
    }

    private static (HttpStatusCode statusCode, string safeMessage) MapException(Exception ex)
    {
        return ex switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Forbidden, "Access denied."),
            KeyNotFoundException => (HttpStatusCode.NotFound, "The requested resource was not found."),
            ArgumentException arg when arg.Message.Contains("password", StringComparison.OrdinalIgnoreCase) => (HttpStatusCode.BadRequest, "Invalid password or request."),
            ArgumentException => (HttpStatusCode.BadRequest, "Invalid request."),
            InvalidOperationException => (HttpStatusCode.BadRequest, "The operation could not be completed."),
            _ => MapGenericException(ex)
        };
    }

    private static (HttpStatusCode statusCode, string safeMessage) MapGenericException(Exception ex)
    {
        var message = ex.Message;
        if (message.Contains("database", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
            ex.StackTrace?.Contains("Npgsql") == true)
            return (HttpStatusCode.ServiceUnavailable, "A temporary error occurred. Please try again later.");

        if (message.Contains("encryption", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("decrypt", StringComparison.OrdinalIgnoreCase))
            return (HttpStatusCode.InternalServerError, "A processing error occurred.");

        if (message.Contains("patient", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("PHI", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("record", StringComparison.OrdinalIgnoreCase))
            return (HttpStatusCode.NotFound, "The requested resource was not found.");

        return (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.");
    }

    /// <summary>
    /// Sanitizes a string so it does not contain emails, UUIDs, phone numbers, or paths in API responses.
    /// Used when we ever need to include any dynamic text in the error body (currently we use fixed messages).
    /// </summary>
    public static string SanitizeForResponse(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        value = Regex.Replace(value, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", "[REDACTED]");
        value = Regex.Replace(value, @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}", "[REDACTED]");
        value = Regex.Replace(value, @"\b\d{3}[-.]?\d{3}[-.]?\d{4}\b", "[REDACTED]");
        value = Regex.Replace(value, @"[A-Za-z]:\\[^\s]+|/[^\s]+", "[REDACTED]");
        return value;
    }
}
