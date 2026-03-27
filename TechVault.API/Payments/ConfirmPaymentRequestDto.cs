namespace TechVault.API.Payments;

public sealed class ConfirmPaymentRequestDto
{
    public string PaymentIntentId { get; set; } = null!;
    public string PaymentMethodId { get; set; } = null!;
}
