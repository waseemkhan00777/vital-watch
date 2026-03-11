namespace VitalCare.Abp;

public static class AuditActions
{
    public const string View = "VIEW";
    public const string Create = "CREATE";
    public const string Update = "UPDATE";
    public const string Delete = "DELETE";
    public const string GrantConsent = "GRANT_CONSENT";
    public const string RevokeConsent = "REVOKE_CONSENT";
    public const string AddAmendment = "ADD_AMENDMENT";
    public const string ExportData = "EXPORT_DATA";
    public const string Login = "LOGIN";
    public const string Logout = "LOGOUT";
}

public static class AuditResourceTypes
{
    public const string Patient = "PATIENT";
    public const string Encounter = "ENCOUNTER";
    public const string Referral = "REFERRAL";
    public const string Consent = "CONSENT";
    public const string User = "USER";
    public const string System = "SYSTEM";
    public const string VitalSign = "VITAL_SIGN";
    public const string Auth = "auth";
}

public interface IAuditService
{
    Task LogAsync(Guid? userId, string? userEmail, string role, string resource, string action, string? resourceId = null, string? details = null, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task LogAsync(Guid? userId, string? userEmail, string role, string resource, string action, string? resourceType, string? dataType, string? dataId, Guid? patientId, string? accessedFields, string? details, string? ipAddress, CancellationToken cancellationToken = default);
    Task LogPHIAccessAsync(Guid? userId, string? userEmail, string role, string resourceType, string dataId, Guid? patientId, string? accessedFields, CancellationToken cancellationToken = default);
    Task LogLoginAsync(Guid? userId, string? userEmail, string role, CancellationToken cancellationToken = default);
    Task LogLogoutAsync(Guid? userId, string? userEmail, string role, CancellationToken cancellationToken = default);
    Task LogAccessDenialAsync(Guid? userId, string? role, string resource, string? ipAddress, CancellationToken cancellationToken = default);
}
