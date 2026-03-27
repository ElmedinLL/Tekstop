namespace TechVault.API.Orders;

public sealed class OrderDto
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = null!;

    public string Status { get; set; } = null!;

    public decimal SubTotal { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal ShippingAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal Total { get; set; }

    public string Currency { get; set; } = null!;

    public string? PaymentMethod { get; set; }

    public string? CouponCode { get; set; }

    public DateTime PlacedAtUtc { get; set; }

    public DateTime? EstimatedDeliveryUtc { get; set; }

    public IReadOnlyList<OrderItemDto> Items { get; set; } = Array.Empty<OrderItemDto>();
}
