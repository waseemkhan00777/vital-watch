using VitalCare.Abp.DTOs;

namespace VitalCare.Abp;

public interface IAuthAppService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<UserDto?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<UserDto?> GetMeAsync(CancellationToken cancellationToken = default);
    Task LogoutAsync(string? accessToken, CancellationToken cancellationToken = default);
    Task<string> GetCsrfTokenAsync(CancellationToken cancellationToken = default);
    Task<LoginResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> VerifyResetTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default);
    Task<LoginResponse?> ChangePasswordFirstLoginAsync(string tempToken, string newPassword, CancellationToken cancellationToken = default);
}
