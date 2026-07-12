using AuthenticationServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthenticationServer.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        //You're describing how the User entity should be stored in SQL Server.
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // ToTable allows you to enforce naming conventions,
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Email)
                   .HasMaxLength(255)
                   .IsRequired();

            builder.HasIndex(u => u.Email)
                   .IsUnique();

        }
    }
}
