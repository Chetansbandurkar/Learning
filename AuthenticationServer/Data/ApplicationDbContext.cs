

using global::AuthenticationServer.Models;
using Microsoft.EntityFrameworkCore;
namespace AuthenticationServer.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder) 
    {
        // Call the base method to ensure any configurations from the base class are applied
        base.OnModelCreating(builder);
        
        // Automatically finds and applies all implementations of IEntityTypeConfiguration
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public DbSet<User> Users { get; set; }
}
