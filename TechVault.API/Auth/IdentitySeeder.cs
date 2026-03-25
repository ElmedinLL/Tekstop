using Microsoft.AspNetCore.Identity;

namespace TechVault.API.Auth;

public sealed class IdentitySeeder(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager)
{
    public async Task SeedAsync(IdentitySeedOptions options)
    {
        await EnsureRoleExistsAsync("Admin");
        await EnsureRoleExistsAsync("Customer");

        var adminUser = await userManager.FindByEmailAsync(options.AdminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = options.AdminEmail,
                Email = options.AdminEmail,
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

        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            var addRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join("; ", addRoleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to assign Admin role to default admin user: {errors}");
            }
        }
    }

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
