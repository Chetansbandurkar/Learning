using AuthenticationServer.Configuration;
using AuthenticationServer.Data;
using AuthenticationServer.DTO;
using AuthenticationServer.Exceptions;
using AuthenticationServer.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthenticationServer.Services;

public class AuthService(
    ApplicationDbContext context,
    IPasswordHasher passwordHasher,
    IJwtService jwtService,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenHasher refreshTokenHasher,
    IEmailService emailService,
    IOptions<JwtSettings> jwtOptions,
    ILogger<AuthService> logger)
    : IAuthService
{
    private readonly ILogger<AuthService> _logger = logger;
    private readonly IEmailService _emailService = emailService;
    private readonly ApplicationDbContext _context = context;
    private readonly IJwtService _jwtService = jwtService;
    private readonly IPasswordHasher passwordHasher = passwordHasher;
    private readonly IRefreshTokenGenerator refreshTokenGenerator = refreshTokenGenerator;
    private readonly IRefreshTokenHasher refreshTokenHasher = refreshTokenHasher;
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task RegisterAsync(RegisterRequest request)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Role = Role.User,
            PasswordHash = passwordHasher.Hash(request.Password),
            EmailVerified = false
        };

        var verificationToken = refreshTokenGenerator.Generate();

        var hashedVerificationToken =
            refreshTokenHasher.Hash(verificationToken);

        var verificationTokenEntity = new EmailVerificationToken
        {
            UserId = user.Id,
            TokenHash = hashedVerificationToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        try
        {
            _context.Users.Add(user);

            _context.EmailVerificationTokens.Add(
                verificationTokenEntity);

            await _context.SaveChangesAsync();

            // Temporary until we have email service
            Console.WriteLine(
                $"EMAIL VERIFICATION TOKEN: {verificationToken}");
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is SqlException sqlEx &&
                (sqlEx.Number == 2601 || sqlEx.Number == 2627))
            {
                throw new EmailAlreadyExistsException(request.Email);
            }

            throw;
        }
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            throw new InvalidCredentialsException(
                "Invalid email or password.");
        }

        // 1. Check whether account is locked
        if (user.LockoutEnd.HasValue &&
            user.LockoutEnd > DateTime.UtcNow)
        {
            throw new InvalidCredentialsException(
                "Account is temporarily locked. Please try again later.");
        }

        // 2. Verify password
        bool passwordValid = passwordHasher.Verify(
            request.Password,
            user.PasswordHash);

        if (!passwordValid)
        {
            user.FailedLoginAttempts++;

            // 3. Lock account after 5 failures
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEnd =
                    DateTime.UtcNow.AddMinutes(15);

                user.FailedLoginAttempts = 0;
            }

            await _context.SaveChangesAsync();

            throw new InvalidCredentialsException(
                "Invalid email or password.");
        }

        // 4. Successful login → reset failed attempts
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;

        // 5. Email verification
        if (!user.EmailVerified)
        {
            throw new InvalidOperationException(
                "Please verify your email before logging in.");
        }

        // 6. Generate access token
        var accessToken = _jwtService.GenerateToken(user);

        // 7. Generate refresh token
        var refreshToken = refreshTokenGenerator.Generate();

        var hashedRefreshToken =
            refreshTokenHasher.Hash(refreshToken);

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = hashedRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(
                _jwtSettings.RefreshTokenExpiryDays)
        };

        _context.RefreshTokens.Add(refreshTokenEntity);

        await _context.SaveChangesAsync();

        return new LoginResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<LoginResponse> RefreshAsync(
    RefreshRequest request)
    {
        // 1. Hash incoming refresh token
        var hashedToken =
            refreshTokenHasher.Hash(request.RefreshToken);

        // 2. Find refresh token
        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt =>
                rt.TokenHash == hashedToken);

        // 3. Token doesn't exist
        if (storedToken == null)
        {
            _logger.LogWarning(
                "Refresh token validation failed: token not found.");

            throw new InvalidRefreshTokenException("Invalid refresh token.");
        }

        // 4. 🚨 Detect refresh token reuse
        if (storedToken.RevokedAt != null)
        {
            _logger.LogWarning(
                "Refresh token reuse detected. UserId: {UserId}, RevokedAt: {RevokedAt}",
                storedToken.UserId,
                storedToken.RevokedAt);

            // Revoke all active refresh tokens
            var activeTokens = await _context.RefreshTokens
                .Where(rt =>
                    rt.UserId == storedToken.UserId &&
                    rt.RevokedAt == null)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "All active refresh tokens revoked after token reuse. UserId: {UserId}",
                storedToken.UserId);

            throw new InvalidRefreshTokenException("Invalid refresh token.");
        }

        // 5. Check expiration
        if (storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogWarning(
                "Expired refresh token used. UserId: {UserId}, ExpiresAt: {ExpiresAt}",
                storedToken.UserId,
                storedToken.ExpiresAt);

            throw new InvalidRefreshTokenException("Invalid refresh token.");
        }

        // 6. Get user
        var user = storedToken.User;

        // 7. Generate new access token
        var newAccessToken =
            _jwtService.GenerateToken(user);

        // 8. Generate new refresh token
        var newRefreshToken =
            refreshTokenGenerator.Generate();

        var hashedNewRefreshToken =
            refreshTokenHasher.Hash(newRefreshToken);

        // 9. Revoke old refresh token
        storedToken.RevokedAt = DateTime.UtcNow;

        // 10. Create new refresh token
        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = hashedNewRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(
                _jwtSettings.RefreshTokenExpiryDays)
        };

        _context.RefreshTokens.Add(newRefreshTokenEntity);

        // 11. Save with concurrency protection
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning(
                "Concurrent refresh attempt detected. UserId: {UserId}",
                user.Id);

            throw new InvalidRefreshTokenException(
                "Refresh token is no longer valid.");
        }

        _logger.LogInformation(
            "Refresh token rotated successfully. UserId: {UserId}",
            user.Id);
        // 12. Return new tokens
        return new LoginResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }
    public async Task LogoutAsync(LogoutRequest request)
    {
        // 1. Hash the incoming refresh token
        var hashedToken = refreshTokenHasher.Hash(request.RefreshToken);

        // 2. Find the refresh token
        // 3. Validate token
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == hashedToken) ?? throw new InvalidRefreshTokenException("Invalid refresh token.");
        if (storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new InvalidRefreshTokenException("Refresh token has expired.");
        }

        if (storedToken.RevokedAt != null)
        {
            throw new InvalidRefreshTokenException("Refresh token has already been revoked.");
        }

        // 4. Revoke token
        storedToken.RevokedAt = DateTime.UtcNow;

        // 5. Save changes
        await _context.SaveChangesAsync();
    }

    public async Task LogoutFromAllDevices(Guid userId)
    {
        // Find all active refresh tokens of this user
        var refreshTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync();

        // Revoke all of them
        foreach (var token in refreshTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        // Save changes
        await _context.SaveChangesAsync();
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        // 1. Find the user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        // Don't reveal whether the email exists
        if (user == null)
        {
            return;
        }

        // 2. Generate a secure random token
        var resetToken = refreshTokenGenerator.Generate();

        // 3. Hash the token before storing it
        var hashedToken = refreshTokenHasher.Hash(resetToken);

        // 4. Create reset-token entity
        var resetTokenEntity = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        // 5. Save hash to database
        _context.PasswordResetTokens.Add(resetTokenEntity);

        await _context.SaveChangesAsync();

        await _emailService.SendPasswordResetEmailAsync(
                user.Email,
                user.FirstName,
                resetToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        // 1. Hash the reset token
        var hashedToken = refreshTokenHasher.Hash(request.Token);

        // 2. Find reset token
        var resetToken = await _context.PasswordResetTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == hashedToken);

        // 3. Validate reset token
        if (resetToken == null)
        {
            throw new InvalidRefreshTokenException("Invalid reset token.");
        }

        if (resetToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new InvalidRefreshTokenException("Reset token has expired.");
        }

        if (resetToken.UsedAt != null)
        {
            throw new InvalidRefreshTokenException(
                "Reset token has already been used.");
        }

        // 4. Get user
        var user = resetToken.User;

        // 5. Update password
        user.PasswordHash = passwordHasher.Hash(request.NewPassword);

        // 6. Mark reset token as used
        resetToken.UsedAt = DateTime.UtcNow;

        // 7. Revoke all active refresh tokens
        var refreshTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
            .ToListAsync();

        foreach (var token in refreshTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        // 8. Save everything
        await _context.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        // 1. Find the logged-in user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new InvalidCredentialsException(
                "User not found.");
        }

        // 2. Verify current password
        bool isCurrentPasswordValid =
            passwordHasher.Verify(
                request.CurrentPassword,
                user.PasswordHash);

        if (!isCurrentPasswordValid)
        {
            throw new InvalidCredentialsException(
                "Current password is incorrect.");
        }

        // 3. Hash the new password
        user.PasswordHash =
            passwordHasher.Hash(request.NewPassword);

        // 4. Revoke all existing refresh tokens
        var refreshTokens = await _context.RefreshTokens
            .Where(rt =>
                rt.UserId == userId &&
                rt.RevokedAt == null)
            .ToListAsync();

        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
        }

        // 5. Save everything
        await _context.SaveChangesAsync();
    }

    public async Task VerifyEmailAsync(
    VerifyEmailRequest request)
    {
        // 1. Hash incoming token
        var hashedToken =
            refreshTokenHasher.Hash(request.Token);

        // 2. Find token
        var verificationToken =
            await _context.EmailVerificationTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(
                    x => x.TokenHash == hashedToken);

        // 3. Validate token
        if (verificationToken == null)
        {
            throw new InvalidRefreshTokenException(
                "Invalid verification token.");
        }

        if (verificationToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new InvalidRefreshTokenException(
                "Verification token has expired.");
        }

        if (verificationToken.UsedAt != null)
        {
            throw new InvalidRefreshTokenException(
                "Verification token has already been used.");
        }

        // 4. Mark email verified
        var user = verificationToken.User;

        user.EmailVerified = true;

        // 5. Mark token as used
        verificationToken.UsedAt = DateTime.UtcNow;

        // 6. Save
        await _context.SaveChangesAsync();
    }
}