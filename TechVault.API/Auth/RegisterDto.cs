using System.ComponentModel.DataAnnotations;

namespace TechVault.API.Auth;

public sealed class RegisterDto
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(2)]
    [MaxLength(50)]
    public string FirstName { get; set; } = null!;

    [Required]
    [MinLength(2)]
    [MaxLength(50)]
    public string LastName { get; set; } = null!;

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
}

