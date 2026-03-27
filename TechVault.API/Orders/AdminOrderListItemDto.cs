namespace TechVault.API.Orders;

public sealed class AdminOrderListItemDto
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime PlacedAtUtc { get; set; }

    public decimal Total { get; set; }

    public string Currency { get; set; } = "USD";

    public int LineItemCount { get; set; }

    /// <summary>From domain user, or Identity when the order has no domain profile link.</summary>
    public string? CustomerEmail { get; set; }
}
