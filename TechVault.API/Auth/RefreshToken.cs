namespace TechVault.API.Auth;

public sealed class RefreshToken
{
    public int Id { get; set; }

    public string Token { get; set; } = null!;

    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }
    public bool IsRevoked => RevokedAtUtc.HasValue;
}

