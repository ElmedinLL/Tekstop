using System.ComponentModel.DataAnnotations;

namespace TechVault.API.Users;

public sealed class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = null!;

    [Required]
    [MinLength(8, ErrorMessage = "New password must be at least 8 characters.")]
    public string NewPassword { get; set; } = null!;
}
