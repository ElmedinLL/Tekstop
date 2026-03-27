using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechVault.API.Data;
using TechVault.API.Models;
using TechVault.API.Models.Enums;
using TechVault.API.Orders;

namespace TechVault.API.Services;

public sealed class AdminOrderService(
    ApplicationDbContext db,
    AuthDbContext authDb,
    IOrderNotificationService orderNotificationService,
    ILogger<AdminOrderService> logger) : IAdminOrderService
{
    private const int MaxPageSize = 100;

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
            ConfirmedAtUtc = order.ConfirmedAtUtc,
            ProcessingAtUtc = order.ProcessingAtUtc,
            PaidAtUtc = order.PaidAtUtc,
            EstimatedDeliveryUtc = estimated,
            ShippedAtUtc = order.ShippedAtUtc,
            DeliveredAtUtc = order.DeliveredAtUtc,
            CancelledAtUtc = order.CancelledAtUtc,
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

    public async Task UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto dto, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<OrderStatus>(dto.Status, ignoreCase: true, out var newStatus))
        {
            throw new InvalidOperationException("Invalid status value.");
        }

        if (newStatus == OrderStatus.PendingPayment)
        {
            throw new InvalidOperationException("Cannot set status to PendingPayment.");
        }

        if (newStatus == OrderStatus.Shipped)
        {
            if (string.IsNullOrWhiteSpace(dto.TrackingUrl))
            {
                throw new InvalidOperationException("Tracking URL is required when status is Shipped.");
            }

            await MarkOrderShippedAsync(orderId, dto.TrackingUrl, cancellationToken);
            return;
        }

        var order = await db.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
        {
            throw new KeyNotFoundException("Order was not found.");
        }

        if (order.Status == newStatus)
        {
            return;
        }

        ApplyAdminStatusChange(order, newStatus);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static void ApplyAdminStatusChange(Order order, OrderStatus newStatus)
    {
        var now = DateTime.UtcNow;
        order.Status = newStatus;
        switch (newStatus)
        {
            case OrderStatus.Confirmed:
                order.ConfirmedAtUtc ??= now;
                break;
            case OrderStatus.Processing:
                order.ProcessingAtUtc ??= now;
                break;
            case OrderStatus.Paid:
                order.PaidAtUtc ??= now;
                break;
            case OrderStatus.Delivered:
                order.DeliveredAtUtc ??= now;
                break;
            case OrderStatus.Cancelled:
                order.CancelledAtUtc ??= now;
                break;
            case OrderStatus.Refunded:
                order.CancelledAtUtc ??= now;
                break;
            case OrderStatus.Pending:
            case OrderStatus.Shipped:
            case OrderStatus.PendingPayment:
                break;
            default:
                break;
        }
    }

    public async Task<AdminOrderListResult> ListOrdersAsync(
        OrderStatus? status,
        DateTime? fromPlacedUtc,
        DateTime? toPlacedUtc,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = db.Orders.AsNoTracking().AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        if (fromPlacedUtc.HasValue)
        {
            var start = DateTime.SpecifyKind(fromPlacedUtc.Value.Date, DateTimeKind.Utc);
            query = query.Where(o => o.PlacedAtUtc >= start);
        }

        if (toPlacedUtc.HasValue)
        {
            var endExclusive = DateTime.SpecifyKind(toPlacedUtc.Value.Date.AddDays(1), DateTimeKind.Utc);
            query = query.Where(o => o.PlacedAtUtc < endExclusive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            if (term.Length > 256)
            {
                term = term[..256];
            }

            if (long.TryParse(term, out var oid) && oid >= int.MinValue && oid <= int.MaxValue)
            {
                var id = (int)oid;
                query = query.Where(o => o.Id == id || o.OrderNumber.Contains(term));
            }
            else
            {
                var identityIdsForSearch = await authDb.Users.AsNoTracking()
                    .Where(u => u.Email != null && u.Email.Contains(term))
                    .Select(u => u.Id)
                    .ToListAsync(cancellationToken);

                query = query.Where(o =>
                    o.OrderNumber.Contains(term) ||
                    (o.User != null && o.User.Email.Contains(term)) ||
                    (o.IdentityUserId != null && identityIdsForSearch.Contains(o.IdentityUserId)));
            }
        }

        query = query.OrderByDescending(o => o.PlacedAtUtc);

        var totalCount = await query.CountAsync(cancellationToken);

        var rows = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                o.Status,
                o.Total,
                o.Currency,
                o.PlacedAtUtc,
                o.IdentityUserId,
                LineItemCount = o.OrderItems.Count,
                DomainEmail = o.User != null ? o.User.Email : null
            })
            .ToListAsync(cancellationToken);

        var identityIds = rows
            .Where(r => r.DomainEmail == null && !string.IsNullOrEmpty(r.IdentityUserId))
            .Select(r => r.IdentityUserId!)
            .Distinct()
            .ToList();

        IReadOnlyDictionary<string, string?> identityEmails = new Dictionary<string, string?>();
        if (identityIds.Count > 0)
        {
            identityEmails = await authDb.Users.AsNoTracking()
                .Where(u => identityIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Email })
                .ToDictionaryAsync(x => x.Id, x => x.Email, cancellationToken);
        }

        var items = rows.Select(r => new AdminOrderListItemDto
        {
            Id = r.Id,
            OrderNumber = r.OrderNumber,
            Status = r.Status.ToString(),
            PlacedAtUtc = r.PlacedAtUtc,
            Total = r.Total,
            Currency = r.Currency,
            LineItemCount = r.LineItemCount,
            CustomerEmail = r.DomainEmail
                ?? (r.IdentityUserId != null && identityEmails.TryGetValue(r.IdentityUserId, out var em) ? em : null)
        }).ToList();

        return new AdminOrderListResult
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
