namespace TechVault.API.Orders;

/// <summary>Admin view of an order including customer identity and shipping snapshot.</summary>
public sealed class AdminOrderDetailDto
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? CustomerEmail { get; set; }

    public string? CustomerName { get; set; }

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

    public DateTime? ShippedAtUtc { get; set; }

    public string? TrackingUrl { get; set; }

    public string ShippingFullName { get; set; } = null!;

    public string ShippingLine1 { get; set; } = null!;

    public string? ShippingLine2 { get; set; }

    public string ShippingCity { get; set; } = null!;

    public string? ShippingRegion { get; set; }

    public string ShippingPostalCode { get; set; } = null!;

    public string ShippingCountry { get; set; } = null!;

    public string? ShippingPhone { get; set; }

    public IReadOnlyList<OrderItemDto> Items { get; set; } = Array.Empty<OrderItemDto>();
}
