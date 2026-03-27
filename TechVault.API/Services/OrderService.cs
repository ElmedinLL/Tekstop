using Microsoft.EntityFrameworkCore;
using TechVault.API.Data;
using TechVault.API.Models;
using TechVault.API.Models.Enums;
using TechVault.API.Orders;

namespace TechVault.API.Services;

public sealed class OrderService(ApplicationDbContext db) : IOrderService
{
    public async Task<OrderDetailDto> CreateAsync(
        string identityUserId,
        CreateOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateDto(dto);

        if (dto.TaxAmount < 0 || dto.ShippingAmount < 0)
        {
            throw new InvalidOperationException("Tax and shipping amounts cannot be negative.");
        }

        var cartItems = await db.CartItems
            .Include(c => c.Product)
            .Where(c => c.IdentityUserId == identityUserId)
            .ToListAsync(cancellationToken);

        if (cartItems.Count == 0)
        {
            throw new InvalidOperationException("Your cart is empty.");
        }

        decimal subTotal = 0;
        foreach (var ci in cartItems)
        {
            var p = ci.Product;
            if (p is null || !p.IsPublished || p.IsDeleted)
            {
                throw new InvalidOperationException("One or more products are no longer available.");
            }

            if (ci.Quantity > p.StockQuantity)
            {
                throw new InvalidOperationException(
                    $"Insufficient stock for \"{p.Name}\". Only {p.StockQuantity} available.");
            }

            subTotal += p.Price * ci.Quantity;
        }

        var total = subTotal + dto.TaxAmount + dto.ShippingAmount;
        if (total < 0)
        {
            throw new InvalidOperationException("Order total is invalid.");
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var order = new Order
            {
                OrderNumber = Guid.NewGuid().ToString("N"),
                IdentityUserId = identityUserId,
                UserId = null,
                Status = OrderStatus.PendingPayment,
                SubTotal = subTotal,
                TaxAmount = dto.TaxAmount,
                ShippingAmount = dto.ShippingAmount,
                Total = total,
                Currency = "USD",
                ShippingAddressId = null,
                ShippingFullName = dto.ShippingFullName.Trim(),
                ShippingLine1 = dto.ShippingLine1.Trim(),
                ShippingLine2 = string.IsNullOrWhiteSpace(dto.ShippingLine2) ? null : dto.ShippingLine2.Trim(),
                ShippingCity = dto.ShippingCity.Trim(),
                ShippingRegion = string.IsNullOrWhiteSpace(dto.ShippingRegion) ? null : dto.ShippingRegion.Trim(),
                ShippingPostalCode = dto.ShippingPostalCode.Trim(),
                ShippingCountry = dto.ShippingCountry.Trim(),
                ShippingPhone = string.IsNullOrWhiteSpace(dto.ShippingPhone) ? null : dto.ShippingPhone.Trim(),
                BillingFullName = dto.BillingFullName.Trim(),
                BillingLine1 = dto.BillingLine1.Trim(),
                BillingLine2 = string.IsNullOrWhiteSpace(dto.BillingLine2) ? null : dto.BillingLine2.Trim(),
                BillingCity = dto.BillingCity.Trim(),
                BillingRegion = string.IsNullOrWhiteSpace(dto.BillingRegion) ? null : dto.BillingRegion.Trim(),
                BillingPostalCode = dto.BillingPostalCode.Trim(),
                BillingCountry = dto.BillingCountry.Trim(),
                PlacedAtUtc = DateTime.UtcNow
            };

            foreach (var ci in cartItems)
            {
                var p = ci.Product!;
                var unit = p.Price;
                var lineTotal = unit * ci.Quantity;
                order.OrderItems.Add(
                    new OrderItem
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        ProductSku = p.Sku,
                        UnitPrice = unit,
                        Quantity = ci.Quantity,
                        LineTotal = lineTotal
                    });
                p.StockQuantity -= ci.Quantity;
            }

            db.Orders.Add(order);
            db.CartItems.RemoveRange(cartItems);
            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            var detail = await MapDetailAsync(order.Id, identityUserId, cancellationToken);
            if (detail is null)
            {
                throw new InvalidOperationException("Order could not be loaded after creation.");
            }

            return detail;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IReadOnlyList<OrderListItemDto>> ListAsync(
        string identityUserId,
        CancellationToken cancellationToken = default)
    {
        var orders = await db.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .Where(o => o.IdentityUserId == identityUserId)
            .OrderByDescending(o => o.PlacedAtUtc)
            .ToListAsync(cancellationToken);

        return orders
            .Select(
                o => new OrderListItemDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    Status = o.Status.ToString(),
                    PlacedAtUtc = o.PlacedAtUtc,
                    SubTotal = o.SubTotal,
                    TaxAmount = o.TaxAmount,
                    ShippingAmount = o.ShippingAmount,
                    Total = o.Total,
                    Currency = o.Currency,
                    ItemCount = o.OrderItems.Count
                })
            .ToList();
    }

    public Task<OrderDetailDto?> GetByIdAsync(
        string identityUserId,
        int orderId,
        CancellationToken cancellationToken = default) =>
        MapDetailAsync(orderId, identityUserId, cancellationToken);

    public async Task<OrderDetailDto?> CancelAsync(
        string identityUserId,
        int orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await db.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(
                o => o.Id == orderId && o.IdentityUserId == identityUserId,
                cancellationToken);

        if (order is null)
        {
            return null;
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Order is already cancelled.");
        }

        if (order.Status != OrderStatus.PendingPayment && order.Status != OrderStatus.Confirmed)
        {
            throw new InvalidOperationException("Only pending or confirmed orders can be cancelled.");
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
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
            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return await MapDetailAsync(order.Id, identityUserId, cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<OrderDetailDto?> MapDetailAsync(
        int orderId,
        string identityUserId,
        CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .Where(o => o.Id == orderId && o.IdentityUserId == identityUserId)
            .FirstOrDefaultAsync(cancellationToken);
        if (order is null)
        {
            return null;
        }

        return new OrderDetailDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            PlacedAtUtc = order.PlacedAtUtc,
            PaidAtUtc = order.PaidAtUtc,
            ShippedAtUtc = order.ShippedAtUtc,
            DeliveredAtUtc = order.DeliveredAtUtc,
            SubTotal = order.SubTotal,
            TaxAmount = order.TaxAmount,
            ShippingAmount = order.ShippingAmount,
            Total = order.Total,
            Currency = order.Currency,
            ShippingFullName = order.ShippingFullName,
            ShippingLine1 = order.ShippingLine1,
            ShippingLine2 = order.ShippingLine2,
            ShippingCity = order.ShippingCity,
            ShippingRegion = order.ShippingRegion,
            ShippingPostalCode = order.ShippingPostalCode,
            ShippingCountry = order.ShippingCountry,
            ShippingPhone = order.ShippingPhone,
            BillingFullName = order.BillingFullName,
            BillingLine1 = order.BillingLine1,
            BillingLine2 = order.BillingLine2,
            BillingCity = order.BillingCity,
            BillingRegion = order.BillingRegion,
            BillingPostalCode = order.BillingPostalCode,
            BillingCountry = order.BillingCountry,
            Lines = order.OrderItems
                .OrderBy(i => i.Id)
                .Select(
                    i => new OrderLineDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        ProductSku = i.ProductSku,
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity,
                        LineTotal = i.LineTotal
                    })
                .ToList()
        };
    }

    private static void ValidateCreateDto(CreateOrderDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ShippingFullName)
            || string.IsNullOrWhiteSpace(dto.ShippingLine1)
            || string.IsNullOrWhiteSpace(dto.ShippingCity)
            || string.IsNullOrWhiteSpace(dto.ShippingPostalCode)
            || string.IsNullOrWhiteSpace(dto.ShippingCountry)
            || string.IsNullOrWhiteSpace(dto.BillingFullName)
            || string.IsNullOrWhiteSpace(dto.BillingLine1)
            || string.IsNullOrWhiteSpace(dto.BillingCity)
            || string.IsNullOrWhiteSpace(dto.BillingPostalCode)
            || string.IsNullOrWhiteSpace(dto.BillingCountry))
        {
            throw new InvalidOperationException("Shipping and billing address fields are required.");
        }
    }
}
