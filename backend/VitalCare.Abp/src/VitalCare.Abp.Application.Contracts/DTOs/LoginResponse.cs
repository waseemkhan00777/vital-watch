namespace VitalCare.Abp.DTOs;

public record LoginResponse(
    string? Token,
    string? RefreshToken,
    string? CsrfToken,
    UserDto User,
    bool RequiresPasswordChange = false,
    string? TempToken = null);
