using global::AuthenticationServer.Models;
using Microsoft.EntityFrameworkCore;
namespace AuthenticationServer.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder) 
    {   
        // Call the base method to ensure any configurations from the base class are applied
        base.OnModelCreating(builder);
        
        // Automatically finds and applies all implementations of IEntityTypeConfiguration
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
}
