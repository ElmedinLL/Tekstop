namespace TechVault.API.Orders;

public sealed class OrderListItemDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime PlacedAtUtc { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = null!;
    public int ItemCount { get; set; }
}
