namespace TechVault.API.Users;

public sealed class UpdateUserProfileDto
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Phone { get; set; }
}
