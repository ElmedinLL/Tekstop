namespace TechVault.API.Models.Enums;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5,

    /// <summary>Stripe / async payment pending.</summary>
    PendingPayment = 6,

    /// <summary>Payment captured (e.g. Stripe webhook).</summary>
    Paid = 7,

    Refunded = 8
}
