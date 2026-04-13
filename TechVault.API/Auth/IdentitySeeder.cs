using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TechVault.API.Auth;

public sealed class IdentitySeeder(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IHostEnvironment hostEnvironment,
    ILogger<IdentitySeeder> logger)
{
    public async Task SeedAsync(IdentitySeedOptions options)
    {
        await EnsureRoleExistsAsync("Admin");
        await EnsureRoleExistsAsync("Customer");

        if (string.IsNullOrWhiteSpace(options.AdminEmail) || string.IsNullOrWhiteSpace(options.AdminPassword))
        {
            logger.LogWarning(
                "IdentitySeed: AdminEmail or AdminPassword is empty; skipping admin user. Set IdentitySeed in appsettings (see appsettings.example.json).");
            return;
        }

        var adminEmail = options.AdminEmail.Trim();
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        // Clear lockout so earlier failed logins cannot block the next admin sign-in after restart.
        if (adminUser is not null && userManager.SupportsUserLockout)
        {
            await userManager.SetLockoutEndDateAsync(adminUser, null);
            await userManager.ResetAccessFailedCountAsync(adminUser);
        }
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = options.AdminFirstName,
                LastName = options.AdminLastName,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, options.AdminPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create default admin user: {errors}");
            }
        }
        else if (ShouldSyncAdminPassword(options))
        {
            var hasPassword = await userManager.HasPasswordAsync(adminUser);
            if (hasPassword)
            {
                var removeResult = await userManager.RemovePasswordAsync(adminUser);
                if (!removeResult.Succeeded)
                {
                    var errors = string.Join("; ", removeResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to clear password for admin sync: {errors}");
                }
            }

            var addResult = await userManager.AddPasswordAsync(adminUser, options.AdminPassword);
            if (!addResult.Succeeded)
            {
                var errors = string.Join("; ", addResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to set admin password from IdentitySeed: {errors}");
            }

            if (userManager.SupportsUserLockout)
            {
                await userManager.SetLockoutEndDateAsync(adminUser, null);
                await userManager.ResetAccessFailedCountAsync(adminUser);
            }

            logger.LogInformation(
                "Identity seed: password and lockout synced for admin {Email} (Development or SyncAdminPassword=true). " +
                "Sign in with this email and the password from IdentitySeed:AdminPassword — not an older password from registration.",
                adminEmail);
        }

        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            var addRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join("; ", addRoleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to assign Admin role to default admin user: {errors}");
            }
        }

        // Clear lockout so failed login attempts cannot block the seeded admin after restart.
        if (userManager.SupportsUserLockout)
        {
            await userManager.SetLockoutEndDateAsync(adminUser, null);
            await userManager.ResetAccessFailedCountAsync(adminUser);
        }

        logger.LogInformation("Identity seed: admin user {Email} has Admin role.", adminUser.Email);
    }

    private bool ShouldSyncAdminPassword(IdentitySeedOptions options) =>
        options.SyncAdminPassword || hostEnvironment.IsDevelopment();

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var createResult = await roleManager.CreateAsync(new IdentityRole(roleName));
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create role '{roleName}': {errors}");
        }
    }
}
