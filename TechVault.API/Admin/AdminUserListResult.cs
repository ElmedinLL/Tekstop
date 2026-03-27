namespace TechVault.API.Admin;

public sealed class AdminUserListResult
{
    public IReadOnlyList<AdminUserListItemDto> Items { get; set; } = Array.Empty<AdminUserListItemDto>();

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }
}
