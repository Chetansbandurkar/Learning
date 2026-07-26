using System.ComponentModel.DataAnnotations;

namespace AuthenticationServer.Models;

public class RefreshToken
{
    // Primary key for the RefreshToken entity
    public Guid Id { get; set; }

    // foreign key to the User entity
    public Guid UserId { get; set; }

    // Navigation property to the User entity

    public User User { get; set; } = null!;

    [MaxLength(500)]
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedAt { get; set; }
}