namespace VitalCare.Abp.DTOs;

public record ResetPasswordRequest(string Token, string NewPassword);
