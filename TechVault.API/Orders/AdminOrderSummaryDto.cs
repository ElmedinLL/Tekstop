namespace TechVault.API.Orders;

public sealed class AdminOrderSummaryDto
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = null!;

    public string CustomerEmail { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public DateTime PlacedAtUtc { get; set; }

    public decimal Total { get; set; }

    public string Currency { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int LineItemCount { get; set; }
}
