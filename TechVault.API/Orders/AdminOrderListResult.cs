namespace TechVault.API.Orders;

public sealed class AdminOrderListResult
{
    public IReadOnlyList<AdminOrderListItemDto> Items { get; set; } = Array.Empty<AdminOrderListItemDto>();

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }
}
