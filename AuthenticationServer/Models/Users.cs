using System.ComponentModel.DataAnnotations;

namespace AuthenticationServer.Models;

using System.ComponentModel.DataAnnotations;

public class User
{
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public Role Role { get; set; } = Role.User;
    public Guid Id { get; set; }

    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool EmailVerified { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property for the related RefreshToken entities , One to many relationship as we one user can have many refresh tokens
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; }
    = new List<PasswordResetToken>();

    public ICollection<EmailVerificationToken> EmailVerificationTokens
    { get; set; } = new List<EmailVerificationToken>();
}