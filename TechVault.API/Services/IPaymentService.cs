using Stripe;
using TechVault.API.Payments;

namespace TechVault.API.Services;

public interface IPaymentService
{
    Task<PaymentIntentCreateResult> CreatePaymentIntentAsync(
        decimal amount,
        int orderId,
        string identityUserId,
        CancellationToken cancellationToken = default);

    Task<PaymentIntent> ConfirmPaymentAsync(
        string paymentIntentId,
        string paymentMethodId,
        CancellationToken cancellationToken = default);

    Task HandleWebhookAsync(Event stripeEvent, CancellationToken cancellationToken = default);
}
