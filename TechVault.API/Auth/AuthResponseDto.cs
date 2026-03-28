namespace TechVault.API.Auth;

public sealed class AuthResponseDto
{
    public string AccessToken { get; set; } = null!;

    public DateTime AccessTokenExpiresAtUtc { get; set; }

    public string RefreshToken { get; set; } = null!;
}

