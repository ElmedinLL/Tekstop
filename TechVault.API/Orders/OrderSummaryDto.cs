namespace TechVault.API.Orders;

public sealed class OrderSummaryDto
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = null!;

    public string Status { get; set; } = null!;

    public decimal Total { get; set; }

    public DateTime PlacedAtUtc { get; set; }

    public int LineItemCount { get; set; }
}
