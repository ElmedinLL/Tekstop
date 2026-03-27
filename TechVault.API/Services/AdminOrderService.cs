using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechVault.API.Data;
using TechVault.API.Models.Enums;
using TechVault.API.Orders;

namespace TechVault.API.Services;

public sealed class AdminOrderService(
    ApplicationDbContext db,
    IOrderNotificationService orderNotificationService,
    ILogger<AdminOrderService> logger) : IAdminOrderService
{
    private const int PageSize = 15;

    public async Task<AdminOrderListResult> ListOrdersAsync(
        int page,
        string? status,
        string? fromDate,
        string? toDate,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);

        if (!string.IsNullOrWhiteSpace(status)
            && !status.Equals("all", StringComparison.OrdinalIgnoreCase)
            && !Enum.TryParse<OrderStatus>(status, true, out _))
        {
            throw new ArgumentException("Invalid status filter.");
        }

        var query = db.Orders.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            var parsed = Enum.Parse<OrderStatus>(status, true);
            query = query.Where(o => o.Status == parsed);
        }

        var fromUtc = ParseDateStartUtc(fromDate);
        var toExclusive = ParseDateEndExclusiveUtc(toDate);

        if (fromUtc.HasValue)
        {
            query = query.Where(o => o.PlacedAtUtc >= fromUtc.Value);
        }

        if (toExclusive.HasValue)
        {
            query = query.Where(o => o.PlacedAtUtc < toExclusive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderByDescending(o => o.PlacedAtUtc)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(o => new AdminOrderSummaryDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerEmail = o.User != null ? o.User.Email : "",
                CustomerName = o.User != null
                    ? o.User.FirstName + " " + o.User.LastName
                    : "—",
                PlacedAtUtc = o.PlacedAtUtc,
                Total = o.Total,
                Currency = o.Currency,
                Status = o.Status.ToString(),
                LineItemCount = o.OrderItems.Count
            })
            .ToListAsync(cancellationToken);

        return new AdminOrderListResult
        {
            Items = rows,
            Page = page,
            PageSize = PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<AdminOrderDetailDto?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
        {
            return null;
        }

        var items = order.OrderItems
            .Select(oi => new OrderItemDto
            {
                ProductId = oi.ProductId,
                ProductName = oi.ProductName,
                ProductSku = oi.ProductSku,
                UnitPrice = oi.UnitPrice,
                Quantity = oi.Quantity,
                LineTotal = oi.LineTotal
            })
            .ToList();

        var estimated = order.PlacedAtUtc.AddDays(5);

        return new AdminOrderDetailDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            CustomerEmail = order.User?.Email,
            CustomerName = order.User != null
                ? $"{order.User.FirstName} {order.User.LastName}".Trim()
                : null,
            SubTotal = order.SubTotal,
            TaxAmount = order.TaxAmount,
            ShippingAmount = order.ShippingAmount,
            DiscountAmount = order.DiscountAmount,
            Total = order.Total,
            Currency = order.Currency,
            PaymentMethod = order.PaymentMethod,
            CouponCode = order.CouponCode,
            PlacedAtUtc = order.PlacedAtUtc,
            EstimatedDeliveryUtc = estimated,
            ShippedAtUtc = order.ShippedAtUtc,
            TrackingUrl = order.TrackingUrl,
            ShippingFullName = order.ShippingFullName,
            ShippingLine1 = order.ShippingLine1,
            ShippingLine2 = order.ShippingLine2,
            ShippingCity = order.ShippingCity,
            ShippingRegion = order.ShippingRegion,
            ShippingPostalCode = order.ShippingPostalCode,
            ShippingCountry = order.ShippingCountry,
            ShippingPhone = order.ShippingPhone,
            Items = items
        };
    }

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

    private static DateTime? ParseDateStartUtc(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return null;
        }

        return DateOnly.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
            ? DateTime.SpecifyKind(d.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)
            : null;
    }

    private static DateTime? ParseDateEndExclusiveUtc(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return null;
        }

        if (!DateOnly.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
        {
            return null;
        }

        var next = d.AddDays(1);
        return DateTime.SpecifyKind(next.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
    }
}
