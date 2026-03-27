using TechVault.API.Models.Enums;

namespace TechVault.API.Models;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public int? UserId { get; set; }
    public string? IdentityUserId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.PendingPayment;

    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = "USD";

    public string? CouponCode { get; set; }
    public string? PaymentMethod { get; set; }

    public int? ShippingAddressId { get; set; }
    public string ShippingFullName { get; set; } = null!;
    public string ShippingLine1 { get; set; } = null!;
    public string? ShippingLine2 { get; set; }
    public string ShippingCity { get; set; } = null!;
    public string? ShippingRegion { get; set; }
    public string ShippingPostalCode { get; set; } = null!;
    public string ShippingCountry { get; set; } = null!;
    public string? ShippingPhone { get; set; }

    public string BillingFullName { get; set; } = null!;
    public string BillingLine1 { get; set; } = null!;
    public string? BillingLine2 { get; set; }
    public string BillingCity { get; set; } = null!;
    public string? BillingRegion { get; set; }
    public string BillingPostalCode { get; set; } = null!;
    public string BillingCountry { get; set; } = null!;

    public DateTime PlacedAtUtc { get; set; }
    public DateTime? ConfirmedAtUtc { get; set; }
    public DateTime? ProcessingAtUtc { get; set; }
    public DateTime? PaidAtUtc { get; set; }
    public DateTime? ShippedAtUtc { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }

    public User? User { get; set; }
    public Address? ShippingAddress { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
