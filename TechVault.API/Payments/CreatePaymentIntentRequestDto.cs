namespace TechVault.API.Payments;

public sealed class CreatePaymentIntentRequestDto
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
}
