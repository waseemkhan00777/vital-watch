using System.Text.RegularExpressions;

namespace VitalCare.Abp;

public static class PasswordValidator
{
    private const int MinLength = 12;

    /// <summary>
    /// Validates password complexity: min 12 chars, upper, lower, number, special character.
    /// </summary>
    public static bool IsPasswordComplex(string? password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < MinLength)
            return false;
        if (!Regex.IsMatch(password, @"[A-Z]"))
            return false;
        if (!Regex.IsMatch(password, @"[a-z]"))
            return false;
        if (!Regex.IsMatch(password, @"[0-9]"))
            return false;
        if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
            return false;
        return true;
    }

    public static string ComplexityMessage =>
        "Password must be at least 12 characters and contain uppercase, lowercase, number, and special character.";
}
