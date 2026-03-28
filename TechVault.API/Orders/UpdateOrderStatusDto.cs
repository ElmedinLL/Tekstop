namespace TechVault.API.Orders;

public sealed class UpdateOrderStatusDto
{
    /// <summary>Target order status name (e.g. Paid, Processing, Shipped, Delivered, Cancelled).</summary>
    public string Status { get; set; } = null!;

    /// <summary>Required when status is Shipped: HTTPS tracking URL (same rules as POST /ship).</summary>
    public string? TrackingUrl { get; set; }
}
