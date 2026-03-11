using System.Linq;
using VitalCare.Abp;

namespace VitalCare.Abp.Controllers;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RolesAttribute : Attribute
{
    public string[] AllowedRoles { get; }

    public RolesAttribute(params UserRole[] roles)
    {
        AllowedRoles = roles.Select(r => r.ToRoleString()).ToArray();
    }

    public bool IsAllowed(string? role)
    {
        if (string.IsNullOrEmpty(role)) return false;
        return AllowedRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}

internal static class UserRoleExtensions
{
    public static string ToRoleString(this UserRole role) => role switch
    {
        UserRole.Admin => VitalCareAbpConstants.Roles.Admin,
        UserRole.Clinician => VitalCareAbpConstants.Roles.Clinician,
        UserRole.CareCoordinator => VitalCareAbpConstants.Roles.CareCoordinator,
        UserRole.Patient => VitalCareAbpConstants.Roles.Patient,
        _ => role.ToString().ToLowerInvariant()
    };
}
