using System.ComponentModel.DataAnnotations;

namespace TechVault.API.Auth;

public sealed class LoginDto
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
}

