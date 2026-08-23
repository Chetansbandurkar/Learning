using AuthenticationServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthenticationServer.Data.Configurations;

public class EmailVerificationTokenConfiguration
    : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(
        EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TokenHash)
            .IsRequired();

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(u => u.EmailVerificationTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}