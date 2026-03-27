using Microsoft.EntityFrameworkCore;
using TechVault.API.Data;
using TechVault.API.Models;
using TechVault.API.Models.Enums;
using TechVault.API.Orders;

namespace TechVault.API.Services;

public sealed class OrderService(
    ApplicationDbContext db,
    IDomainUserService domainUserService,
    ICouponValidationService couponValidationService) : IOrderService
{
    private const decimal StandardShipping = 9.99m;
    private const decimal ExpressShipping = 19.99m;
    private const int PageSize = 10;

    public async Task<OrderDto> CreateOrderAsync(
        string identityUserId,
        CreateOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityUserId))
        {
            throw new ArgumentException("Identity user id is required.", nameof(identityUserId));
        }

        var domainUserId = await domainUserService.GetOrCreateDomainUserIdAsync(identityUserId, cancellationToken);

        var address = await db.Addresses
            .AsNoTracking()
            .FirstOrDefaultAsync(
                a => a.Id == dto.AddressId && a.UserId == domainUserId,
                cancellationToken);

        if (address is null)
        {
            throw new InvalidOperationException("Shipping address was not found.");
        }

        var cartItems = await db.CartItems
            .Include(c => c.Product)
            .Where(c => c.IdentityUserId == identityUserId)
            .ToListAsync(cancellationToken);

        if (cartItems.Count == 0)
        {
            throw new InvalidOperationException("Your cart is empty.");
        }

        if (!CartMatchesSnapshot(cartItems, dto.CartItems))
        {
            throw new InvalidOperationException("Cart does not match the server. Refresh and try again.");
        }

        foreach (var line in cartItems)
        {
            if (line.Product is null || !line.Product.IsPublished || line.Product.IsDeleted)
            {
                throw new InvalidOperationException($"Product {line.ProductId} is not available.");
            }

            var qty = Math.Min(line.Quantity, line.Product.StockQuantity);
            if (qty < 1 || line.Quantity > line.Product.StockQuantity)
            {
                throw new InvalidOperationException(
                    $"Insufficient stock for {line.Product.Name}.");
            }
        }

        decimal subTotal = 0;
        foreach (var line in cartItems)
        {
            var p = line.Product!;
            var qty = Math.Min(line.Quantity, p.StockQuantity);
            subTotal += p.Price * qty;
        }

        decimal discountAmount = 0;
        string? couponCode = null;
        if (!string.IsNullOrWhiteSpace(dto.CouponCode))
        {
            var couponResult = await couponValidationService.ValidateAsync(
                dto.CouponCode.Trim(),
                subTotal,
                cancellationToken);

            if (!couponResult.IsValid)
            {
                throw new InvalidOperationException(couponResult.Message);
            }

            discountAmount = couponResult.DiscountAmount;
            couponCode = couponResult.Code;
        }

        var shippingAmount = ResolveShippingAmount(dto.ShippingMethod);
        var taxAmount = 0m;
        var total = subTotal - discountAmount + taxAmount + shippingAmount;
        if (total < 0)
        {
            total = 0;
        }

        var now = DateTime.UtcNow;
        var orderNumber = await GenerateUniqueOrderNumberAsync(cancellationToken);

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var order = new Order
            {
                OrderNumber = orderNumber,
                UserId = domainUserId,
                IdentityUserId = identityUserId,
                Status = OrderStatus.Confirmed,
                SubTotal = subTotal,
                TaxAmount = taxAmount,
                ShippingAmount = shippingAmount,
                DiscountAmount = discountAmount,
                Total = total,
                Currency = "USD",
                CouponCode = couponCode,
                PaymentMethod = dto.PaymentMethod.Trim(),
                ShippingAddressId = address.Id,
                ShippingFullName = address.FullName,
                ShippingLine1 = address.Line1,
                ShippingLine2 = address.Line2,
                ShippingCity = address.City,
                ShippingRegion = address.Region,
                ShippingPostalCode = address.PostalCode,
                ShippingCountry = address.Country,
                ShippingPhone = address.Phone,
                BillingFullName = address.FullName,
                BillingLine1 = address.Line1,
                BillingLine2 = address.Line2,
                BillingCity = address.City,
                BillingRegion = address.Region,
                BillingPostalCode = address.PostalCode,
                BillingCountry = address.Country,
                PlacedAtUtc = now,
                ConfirmedAtUtc = now,
                ProcessingAtUtc = now
            };

            if (string.Equals(dto.PaymentMethod.Trim(), "card", StringComparison.OrdinalIgnoreCase))
            {
                order.PaidAtUtc = now;
            }

            db.Orders.Add(order);
            await db.SaveChangesAsync(cancellationToken);

            foreach (var line in cartItems)
            {
                var p = line.Product!;
                var qty = Math.Min(line.Quantity, p.StockQuantity);
                var unitPrice = p.Price;
                var lineTotal = unitPrice * qty;

                db.OrderItems.Add(
                    new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = p.Id,
                        ProductName = p.Name,
                        ProductSku = p.Sku,
                        UnitPrice = unitPrice,
                        Quantity = qty,
                        LineTotal = lineTotal
                    });

                var tracked = await db.Products.FirstAsync(x => x.Id == p.Id, cancellationToken);
                tracked.StockQuantity -= qty;
            }

            db.CartItems.RemoveRange(cartItems);
            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return await MapOrderDtoAsync(order.Id, identityUserId, cancellationToken)
                ?? throw new InvalidOperationException("Order could not be loaded.");
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<OrderListResult> GetOrdersAsync(
        string identityUserId,
        int page,
        CancellationToken cancellationToken = default)
    {
        var domainUserId = await domainUserService.GetOrCreateDomainUserIdAsync(identityUserId, cancellationToken);
        page = Math.Max(1, page);
        var query = db.Orders
            .AsNoTracking()
            .Where(o => o.UserId == domainUserId)
            .OrderByDescending(o => o.PlacedAtUtc);

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                o.Status,
                o.Total,
                o.PlacedAtUtc,
                LineItemCount = o.OrderItems.Count
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Status = o.Status.ToString(),
                Total = o.Total,
                PlacedAtUtc = o.PlacedAtUtc,
                LineItemCount = o.LineItemCount
            })
            .ToList();

        return new OrderListResult
        {
            Items = items,
            Page = page,
            PageSize = PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<OrderDto?> GetOrderByIdAsync(
        string identityUserId,
        int orderId,
        CancellationToken cancellationToken = default)
    {
        return await MapOrderDtoAsync(orderId, identityUserId, cancellationToken);
    }

    public async Task CancelOrderAsync(
        string identityUserId,
        int orderId,
        CancellationToken cancellationToken = default)
    {
        var domainUserId = await domainUserService.GetOrCreateDomainUserIdAsync(identityUserId, cancellationToken);

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var order = await db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(
                    o => o.Id == orderId && o.UserId == domainUserId,
                    cancellationToken);

            if (order is null)
            {
                throw new InvalidOperationException("Order was not found.");
            }

            if (order.Status is OrderStatus.Shipped or OrderStatus.Delivered or OrderStatus.Cancelled
                or OrderStatus.Refunded)
            {
                throw new InvalidOperationException("This order cannot be cancelled.");
            }

            var productIds = order.OrderItems.Select(i => i.ProductId).Distinct().ToList();
            var products = await db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            foreach (var line in order.OrderItems)
            {
                if (products.TryGetValue(line.ProductId, out var product))
                {
                    product.StockQuantity += line.Quantity;
                }
            }

            order.Status = OrderStatus.Cancelled;
            order.CancelledAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<OrderDto?> MapOrderDtoAsync(
        int orderId,
        string identityUserId,
        CancellationToken cancellationToken)
    {
        var domainUserId = await domainUserService.GetOrCreateDomainUserIdAsync(identityUserId, cancellationToken);

        var order = await db.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(
                o => o.Id == orderId && o.UserId == domainUserId,
                cancellationToken);

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

        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
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
            Items = items
        };
    }

    private static bool CartMatchesSnapshot(
        IReadOnlyList<CartItem> dbItems,
        IReadOnlyList<CreateOrderCartItemDto>? snapshot)
    {
        if (snapshot is null || snapshot.Count == 0)
        {
            return true;
        }

        if (dbItems.Count != snapshot.Count)
        {
            return false;
        }

        var a = dbItems
            .OrderBy(x => x.ProductId)
            .Select(x => (x.ProductId, x.Quantity))
            .ToList();

        var b = snapshot
            .OrderBy(x => x.ProductId)
            .Select(x => (x.ProductId, x.Quantity))
            .ToList();

        return a.SequenceEqual(b);
    }

    private static decimal ResolveShippingAmount(string shippingMethod)
    {
        if (string.Equals(shippingMethod, "express", StringComparison.OrdinalIgnoreCase))
        {
            return ExpressShipping;
        }

        return StandardShipping;
    }

    private async Task<string> GenerateUniqueOrderNumberAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < 8; i++)
        {
            var candidate = $"TV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8]}";
            var exists = await db.Orders.AnyAsync(o => o.OrderNumber == candidate, cancellationToken);
            if (!exists)
            {
                return candidate;
            }
        }

        return $"TV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
    }
}
