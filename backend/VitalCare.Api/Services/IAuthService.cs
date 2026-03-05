using VitalCare.Api.DTOs;

namespace VitalCare.Api.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<UserDto?> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    string GenerateJwt(UserDto user);
}
