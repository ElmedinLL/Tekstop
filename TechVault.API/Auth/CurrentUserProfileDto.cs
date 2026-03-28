namespace TechVault.API.Auth;

public sealed class CurrentUserProfileDto
{
    public string UserId { get; set; } = null!;

    public string Email { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? ProfilePicture { get; set; }

    public string[] Roles { get; set; } = [];
}
