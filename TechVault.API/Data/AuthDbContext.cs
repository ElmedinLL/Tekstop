using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechVault.API.Auth;

namespace TechVault.API.Data;

public class AuthDbContext : IdentityDbContext<ApplicationUser>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FirstName).HasMaxLength(100);
            entity.Property(u => u.LastName).HasMaxLength(100);
            entity.Property(u => u.ProfilePicture).HasMaxLength(2048);

            // Helps keep CreatedAt consistent even if you create users without setting the property explicitly.
            entity.Property(u => u.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("UTC_TIMESTAMP(6)");
        });
    }
}