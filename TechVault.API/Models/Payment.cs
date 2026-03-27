namespace TechVault.API.Models;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    /// <summary>Provider key, e.g. stripe.</summary>
    public string Provider { get; set; } = "stripe";

    /// <summary>Stripe PaymentIntent id (pi_...).</summary>
    public string ExternalPaymentId { get; set; } = null!;

    /// <summary>Stripe Charge id when present (ch_...).</summary>
    public string? ExternalChargeId { get; set; }

    public long AmountCents { get; set; }
    public string Currency { get; set; } = "usd";

    public DateTime CreatedAtUtc { get; set; }
}
