using AuthenticationServer.DTO;
namespace AuthenticationServer.Services;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RefreshAsync(RefreshRequest request);
    Task LogoutAsync(LogoutRequest request);
    Task ForgotPasswordAsync(ForgotPasswordRequest request);
    Task LogoutFromAllDevices(Guid userId);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task ChangePasswordAsync(
    Guid userId,
    ChangePasswordRequest request);
    Task VerifyEmailAsync(VerifyEmailRequest request);
}