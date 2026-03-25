namespace TechVault.API.Auth;

public sealed record JwtTokenResult(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken
);

