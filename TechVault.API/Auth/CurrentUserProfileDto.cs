using System.ComponentModel.DataAnnotations;

namespace TechVault.API.Auth;

public sealed class CurrentUserProfileDto
{
    [Required]
    public string UserId { get; set; } = null!;

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    public string? ProfilePicture { get; set; }

    [Required]
    public string[] Roles { get; set; } = [];
}
