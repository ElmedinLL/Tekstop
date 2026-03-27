using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechVault.API.Data;
using TechVault.API.Models.Enums;

namespace TechVault.API.Services;

public sealed class AdminOrderService(
    ApplicationDbContext db,
    IOrderNotificationService orderNotificationService,
    ILogger<AdminOrderService> logger) : IAdminOrderService
{
    public async Task MarkOrderShippedAsync(int orderId, string trackingUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(trackingUrl))
        {
            throw new InvalidOperationException("Tracking URL is required.");
        }

        var trimmed = trackingUrl.Trim();
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            || uri.Scheme is not ("http" or "https"))
        {
            throw new InvalidOperationException("Tracking URL must be a valid http or https URL.");
        }

        var order = await db.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
        {
            throw new InvalidOperationException("Order was not found.");
        }

        if (order.Status is OrderStatus.Shipped or OrderStatus.Delivered or OrderStatus.Cancelled
            or OrderStatus.Refunded or OrderStatus.PendingPayment)
        {
            throw new InvalidOperationException("Order cannot be shipped in its current state.");
        }

        if (order.Status is OrderStatus.Pending)
        {
            throw new InvalidOperationException("Order cannot be shipped in its current state.");
        }

        order.Status = OrderStatus.Shipped;
        order.ShippedAtUtc = DateTime.UtcNow;
        order.TrackingUrl = trimmed;

        await db.SaveChangesAsync(cancellationToken);

        try
        {
            await orderNotificationService.SendOrderShippedAsync(order, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send order shipped email for order {OrderId}.", order.Id);
        }
    }
}
