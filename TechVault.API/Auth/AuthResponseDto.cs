using System.ComponentModel.DataAnnotations;

namespace TechVault.API.Auth;

public sealed class AuthResponseDto
{
    [Required]
    public string AccessToken { get; set; } = null!;

    public DateTime AccessTokenExpiresAtUtc { get; set; }

    [Required]
    public string RefreshToken { get; set; } = null!;
}

