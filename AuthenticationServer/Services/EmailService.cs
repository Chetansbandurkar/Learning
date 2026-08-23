using AuthenticationServer.Configuration;
using Microsoft.Extensions.Options;
using Resend;

namespace AuthenticationServer.Services;

public class EmailService(
    IResend resend,
    IOptions<EmailSettings> emailOptions)
    : IEmailService
{
    private readonly IResend _resend = resend;
    private readonly EmailSettings _settings = emailOptions.Value;

    public async Task SendVerificationEmailAsync(
        string email,
        string firstName,
        string token)
    {
        var verificationLink =
            $"{_settings.FrontendUrl}/verify-email?token={Uri.EscapeDataString(token)}";

        var message = new EmailMessage
        {
            From = $"{_settings.FromName} <{_settings.FromEmail}>",
            Subject = "Verify your email",
            HtmlBody = $"""
                <h2>Welcome, {firstName}!</h2>

                <p>
                    Thanks for creating an account.
                    Please verify your email address.
                </p>

                <p>
                    <a href="{verificationLink}">
                        Verify Email
                    </a>
                </p>

                <p>
                    This link will expire in 30 minutes.
                </p>
                """
        };

        message.To.Add(email);

        await _resend.EmailSendAsync(message);
    }

    public async Task SendPasswordResetEmailAsync(
        string email,
        string firstName,
        string token)
    {
        var resetLink =
            $"{_settings.FrontendUrl}/reset-password?token={Uri.EscapeDataString(token)}";

        var message = new EmailMessage
        {
            From = $"{_settings.FromName} <{_settings.FromEmail}>",
            Subject = "Reset your password",
            HtmlBody = $"""
                <h2>Password Reset</h2>

                <p>
                    Hello {firstName},
                </p>

                <p>
                    We received a request to reset your password.
                </p>

                <p>
                    <a href="{resetLink}">
                        Reset Password
                    </a>
                </p>

                <p>
                    This link will expire in 15 minutes.
                </p>

                <p>
                    If you did not request this, you can safely ignore this email.
                </p>
                """
        };

        message.To.Add(email);

        await _resend.EmailSendAsync(message);
    }
}