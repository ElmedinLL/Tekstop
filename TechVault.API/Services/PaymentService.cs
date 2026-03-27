using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using TechVault.API.Data;
using TechVault.API.Models;
using TechVault.API.Models.Enums;
using TechVault.API.Payments;

namespace TechVault.API.Services;

public sealed class PaymentService(
    ApplicationDbContext db,
    IOptions<StripeSettings> stripeOptions,
    IOrderNotificationService orderNotificationService,
    ILogger<PaymentService> logger) : IPaymentService
{
    private readonly StripeSettings _stripe = stripeOptions.Value;

    public async Task<PaymentIntentCreateResult> CreatePaymentIntentAsync(
        decimal amount,
        int orderId,
        string identityUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_stripe.SecretKey))
        {
            throw new InvalidOperationException("Stripe is not configured.");
        }

        var order = await db.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(
                o => o.Id == orderId && o.IdentityUserId == identityUserId,
                cancellationToken);

        if (order is null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        if (order.Status != OrderStatus.PendingPayment)
        {
            throw new InvalidOperationException("Order cannot be paid in its current state.");
        }

        if (Math.Abs(order.Total - amount) > 0.01m)
        {
            throw new InvalidOperationException("Amount does not match order total.");
        }

        var amountCents = (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
        if (amountCents < 1)
        {
            throw new InvalidOperationException("Invalid payment amount.");
        }

        var client = new StripeClient(_stripe.SecretKey);
        var service = new PaymentIntentService(client);

        var currency = string.IsNullOrWhiteSpace(_stripe.Currency) ? "usd" : _stripe.Currency.Trim().ToLowerInvariant();

        var options = new PaymentIntentCreateOptions
        {
            Amount = amountCents,
            Currency = currency,
            Metadata = new Dictionary<string, string> { ["orderId"] = orderId.ToString() },
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true }
        };

        var intent = await service.CreateAsync(options, requestOptions: null, cancellationToken);

        return new PaymentIntentCreateResult
        {
            PaymentIntentId = intent.Id,
            ClientSecret = intent.ClientSecret ?? "",
            Amount = intent.Amount,
            Currency = intent.Currency
        };
    }

    public async Task<PaymentIntent> ConfirmPaymentAsync(
        string paymentIntentId,
        string paymentMethodId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_stripe.SecretKey))
        {
            throw new InvalidOperationException("Stripe is not configured.");
        }

        if (string.IsNullOrWhiteSpace(paymentIntentId) || string.IsNullOrWhiteSpace(paymentMethodId))
        {
            throw new InvalidOperationException("Payment intent and payment method are required.");
        }

        var client = new StripeClient(_stripe.SecretKey);
        var service = new PaymentIntentService(client);

        var options = new PaymentIntentConfirmOptions
        {
            PaymentMethod = paymentMethodId
        };

        return await service.ConfirmAsync(paymentIntentId, options, requestOptions: null, cancellationToken);
    }

    public async Task HandleWebhookAsync(Event stripeEvent, CancellationToken cancellationToken = default)
    {
        if (stripeEvent.Type != EventTypes.PaymentIntentSucceeded)
        {
            return;
        }

        if (stripeEvent.Data.Object is not PaymentIntent pi)
        {
            return;
        }

        if (await db.Payments.AnyAsync(p => p.ExternalPaymentId == pi.Id, cancellationToken))
        {
            return;
        }

        if (!pi.Metadata.TryGetValue("orderId", out var orderIdStr)
            || !int.TryParse(orderIdStr, out var orderId))
        {
            return;
        }

        var order = await db.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null || order.Status != OrderStatus.PendingPayment)
        {
            return;
        }

        var expectedCents = (long)Math.Round(order.Total * 100m, MidpointRounding.AwayFromZero);
        if (pi.Amount != expectedCents)
        {
            return;
        }

        var now = DateTime.UtcNow;
        order.Status = OrderStatus.Confirmed;
        order.ConfirmedAtUtc = now;
        order.PaidAtUtc = now;
        order.ProcessingAtUtc ??= now;

        db.Payments.Add(
            new Payment
            {
                OrderId = order.Id,
                Provider = "stripe",
                ExternalPaymentId = pi.Id,
                ExternalChargeId = string.IsNullOrEmpty(pi.LatestChargeId) ? null : pi.LatestChargeId,
                AmountCents = pi.Amount,
                Currency = string.IsNullOrWhiteSpace(pi.Currency) ? "usd" : pi.Currency,
                CreatedAtUtc = now
            });

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            if (await db.Payments.AnyAsync(p => p.ExternalPaymentId == pi.Id, cancellationToken))
            {
                return;
            }

            throw;
        }

        try
        {
            await orderNotificationService.SendOrderConfirmationAsync(order, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send order confirmation email for order {OrderId}.", order.Id);
        }
    }
}
