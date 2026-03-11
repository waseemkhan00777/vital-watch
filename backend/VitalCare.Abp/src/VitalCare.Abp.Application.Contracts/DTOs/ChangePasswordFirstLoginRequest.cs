namespace VitalCare.Abp.DTOs;

public record ChangePasswordFirstLoginRequest(string TempToken, string NewPassword);
