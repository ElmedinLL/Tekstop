namespace TechVault.API.Orders;

public sealed class AdminOrderListResult
{
    public IReadOnlyList<AdminOrderSummaryDto> Items { get; set; } = Array.Empty<AdminOrderSummaryDto>();

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }
}
