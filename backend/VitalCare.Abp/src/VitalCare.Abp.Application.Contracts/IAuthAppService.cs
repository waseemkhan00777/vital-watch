using VitalCare.Abp.DTOs;

namespace VitalCare.Abp;

public interface IAuthAppService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<UserDto?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<UserDto?> GetMeAsync(CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
}
