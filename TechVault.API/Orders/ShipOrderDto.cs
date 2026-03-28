namespace TechVault.API.Orders;

public sealed class ShipOrderDto
{
    /// <summary>HTTPS link to carrier tracking or order status page.</summary>
    public string TrackingUrl { get; set; } = null!;
}
