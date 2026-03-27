namespace TechVault.API.Admin;

public sealed class AdminUserListItemDto
{
    public string UserId { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();

    public DateTime JoinedAtUtc { get; set; }

    public int OrderCount { get; set; }

    public bool IsBanned { get; set; }
}
