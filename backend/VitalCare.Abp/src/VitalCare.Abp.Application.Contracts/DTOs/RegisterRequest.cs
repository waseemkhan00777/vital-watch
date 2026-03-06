namespace VitalCare.Abp.DTOs;

public record RegisterRequest(string Email, string Password, string Name, string Role = "patient");
