using System.ComponentModel.DataAnnotations;

namespace TechVault.API.Auth;

public sealed class RefreshTokenDto
{
    [Required]
    [MinLength(32)]
    [MaxLength(512)]
    public string RefreshToken { get; set; } = null!;
}

