namespace TechVault.API.Auth;

public interface IJwtTokenService
{
    Task<JwtTokenResult> GenerateToken(ApplicationUser user);
}

