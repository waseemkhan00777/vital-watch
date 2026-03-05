namespace VitalCare.Api.DTOs;

public record LoginRequest(string Email, string Password, string? Role = null);

public record LoginResponse(string Token, UserDto User);

public record UserDto(string Id, string Email, string Role, string Name);

public record RegisterRequest(string Email, string Password, string Name, string Role = "patient");
