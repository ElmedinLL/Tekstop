using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace TechVault.API.Auth;

public sealed class JwtTokenService(
    IOptions<JwtSettings> jwtOptions,
    UserManager<ApplicationUser> userManager)
    : IJwtTokenService
{
    public async Task<JwtTokenResult> GenerateToken(ApplicationUser user)
    {
        var jwt = jwtOptions.Value;

        if (string.IsNullOrWhiteSpace(jwt.Secret) || jwt.Secret.Length < 32)
            throw new InvalidOperationException("JWT Secret must be configured and at least 32 characters.");
        if (string.IsNullOrWhiteSpace(jwt.Issuer))
            throw new InvalidOperationException("JWT Issuer must be configured.");
        if (string.IsNullOrWhiteSpace(jwt.Audience))
            throw new InvalidOperationException("JWT Audience must be configured.");
        if (jwt.ExpiryInDays <= 0)
            throw new InvalidOperationException("JWT ExpiryInDays must be a positive integer.");

        var now = DateTime.UtcNow;
        var expires = now.AddDays(jwt.ExpiryInDays);

        var roles = await userManager.GetRolesAsync(user);
        var customClaims = await userManager.GetClaimsAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(now).ToString(), ClaimValueTypes.Integer64),

            new("UserId", user.Id),
            new("Email", user.Email ?? string.Empty),

            // Useful for app display; not relied on for authorization decisions.
            new("FirstName", user.FirstName),
            new("LastName", user.LastName),
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim("Roles", role));
        }

        foreach (var claim in customClaims)
        {
            // Avoid duplicates for core claims we already emit.
            if (claim.Type is "UserId" or "Email" or "Roles")
                continue;

            claims.Add(claim);
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = GenerateSecureRefreshToken();

        return new JwtTokenResult(accessToken, expires, refreshToken);
    }

    private static string GenerateSecureRefreshToken()
    {
        // 64 bytes => 512 bits of entropy; URL-safe base64.
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Base64UrlEncoder.Encode(bytes);
    }
}

