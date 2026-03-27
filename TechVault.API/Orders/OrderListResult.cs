namespace TechVault.API.Orders;

public sealed class OrderListResult
{
    public IReadOnlyList<OrderSummaryDto> Items { get; set; } = Array.Empty<OrderSummaryDto>();

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }
}
