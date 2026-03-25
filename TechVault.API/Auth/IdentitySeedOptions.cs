namespace TechVault.API.Auth;

public sealed class IdentitySeedOptions
{
    public const string SectionName = "IdentitySeed";

    /// <summary>
    /// When empty, admin user seeding is skipped (roles are still ensured).
    /// </summary>
    public string AdminEmail { get; set; } = "";

    /// <summary>
    /// When empty, admin user seeding is skipped. Never commit a real password; use user secrets or env-specific config.
    /// </summary>
    public string AdminPassword { get; set; } = "";

    public string AdminFirstName { get; set; } = "System";
    public string AdminLastName { get; set; } = "Admin";
}
