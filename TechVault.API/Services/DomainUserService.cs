using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechVault.API.Auth;
using TechVault.API.Data;
using TechVault.API.Models;
using TechVault.API.Models.Enums;

namespace TechVault.API.Services;

public sealed class DomainUserService(
    ApplicationDbContext db,
    UserManager<ApplicationUser> userManager) : IDomainUserService
{
    public async Task<int> GetOrCreateDomainUserIdAsync(string identityUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityUserId))
        {
            throw new ArgumentException("Identity user id is required.", nameof(identityUserId));
        }

        var existingId = await db.Users
            .AsNoTracking()
            .Where(u => u.IdentityUserId == identityUserId)
            .Select(u => (int?)u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingId is { } id)
        {
            return id;
        }

        var appUser = await userManager.FindByIdAsync(identityUserId);
        if (appUser is null)
        {
            throw new InvalidOperationException("Identity user was not found.");
        }

        var email = appUser.Email ?? throw new InvalidOperationException("Identity user email is required.");

        var domainUser = new User
        {
            IdentityUserId = identityUserId,
            Email = email,
            PasswordHash = "__identity__",
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
            PhoneNumber = appUser.PhoneNumber,
            Role = UserRole.Customer,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Users.Add(domainUser);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var retryId = await db.Users
                .AsNoTracking()
                .Where(u => u.IdentityUserId == identityUserId)
                .Select(u => (int?)u.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (retryId is { } rid)
            {
                return rid;
            }

            throw;
        }

        return domainUser.Id;
    }

    public async Task DeleteByIdentityUserIdIfExistsAsync(string identityUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityUserId))
        {
            return;
        }

        var domainUser = await db.Users
            .FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId, cancellationToken);

        if (domainUser is null)
        {
            return;
        }

        db.Users.Remove(domainUser);
        await db.SaveChangesAsync(cancellationToken);
    }
}
