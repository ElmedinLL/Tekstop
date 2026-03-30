namespace TechVault.API.Auth;

/// <summary>Used by the SPA to decide whether to call POST /auth/refresh on load (avoids a pointless 401 when anonymous).</summary>
public sealed class AuthSessionBootstrapDto
{
    public bool HasRefreshCookie { get; init; }
}
