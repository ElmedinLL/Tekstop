namespace TechVault.API.Auth;

public sealed class IdentitySeedOptions
{
    public const string SectionName = "IdentitySeed";

    public string AdminEmail { get; set; } = "admin@techvault.local";
    public string AdminPassword { get; set; } = "Admin1234";
    public string AdminFirstName { get; set; } = "System";
    public string AdminLastName { get; set; } = "Admin";
}
