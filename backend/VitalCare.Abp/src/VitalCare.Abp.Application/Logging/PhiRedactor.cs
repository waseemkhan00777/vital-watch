using System.Text.RegularExpressions;

namespace VitalCare.Abp.Logging;

/// <summary>
/// Redacts PHI and identifiers from strings for safe logging (HIPAA-aware).
/// </summary>
public static class PhiRedactor
{
    private static readonly Regex EmailRegex = new(
        @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}",
        RegexOptions.Compiled);

    private static readonly Regex GuidRegex = new(
        @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
        RegexOptions.Compiled);

    private static readonly Regex PhoneRegex = new(
        @"\b\d{3}[-.\s]?\d{3}[-.\s]?\d{4}\b",
        RegexOptions.Compiled);

    /// <summary>
    /// MRN-like: long digit sequences that might be medical record numbers.
    /// </summary>
    private static readonly Regex MrnLikeRegex = new(
        @"\b\d{8,}\b",
        RegexOptions.Compiled);

    private const string Redacted = "[REDACTED]";

    /// <summary>
    /// Redacts email, GUIDs, phone numbers, and MRN-like sequences from the message.
    /// </summary>
    public static string Redact(string? message)
    {
        if (string.IsNullOrEmpty(message)) return string.Empty;

        message = EmailRegex.Replace(message, Redacted);
        message = GuidRegex.Replace(message, Redacted);
        message = PhoneRegex.Replace(message, Redacted);
        message = MrnLikeRegex.Replace(message, Redacted);
        return message;
    }
}
