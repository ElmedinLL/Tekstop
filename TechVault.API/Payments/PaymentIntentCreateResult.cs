namespace TechVault.API.Payments;

public sealed class PaymentIntentCreateResult
{
    public string PaymentIntentId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public long Amount { get; set; }
    public string Currency { get; set; } = null!;
}
