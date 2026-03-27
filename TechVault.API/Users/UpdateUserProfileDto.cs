using System.ComponentModel.DataAnnotations;

namespace TechVault.API.Users;

public sealed class UpdateUserProfileDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = null!;

    [MaxLength(32)]
    public string? Phone { get; set; }
}
