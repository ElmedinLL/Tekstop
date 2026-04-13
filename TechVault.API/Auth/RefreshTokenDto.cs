namespace TechVault.API.Auth;

/// <summary>Refresh token in body (optional when <see cref="AuthCookieNames.Refresh"/> cookie is sent).</summary>
public sealed class RefreshTokenDto
{
    public string? RefreshToken { get; set; }
}

