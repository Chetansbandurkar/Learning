namespace AuthenticationServer.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(
        string email,
        string firstName,
        string token);

    Task SendPasswordResetEmailAsync(
        string email,
        string firstName,
        string token);
}