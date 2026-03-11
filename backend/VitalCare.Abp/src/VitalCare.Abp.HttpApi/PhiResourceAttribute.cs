namespace VitalCare.Abp.Controllers;

/// <summary>
/// Marks a controller action (or entire controller) as accessing PHI.
/// When present, PhiAccessAuditFilter will automatically write an audit log entry
/// after every successful response, satisfying HIPAA § 164.312(b) audit controls.
///
/// Usage:
///   [PhiResource("patients")]          // on a controller class
///   [PhiResource("vital_signs")]        // on an individual action
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class PhiResourceAttribute(string resource) : Attribute
{
    public string Resource { get; } = resource;
}
