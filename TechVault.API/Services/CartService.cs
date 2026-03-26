using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using TechVault.API.Carts;
using TechVault.API.Data;
using TechVault.API.Models;

namespace TechVault.API.Services;

public sealed class CartService(
    ApplicationDbContext db,
    IDistributedCache cache) : ICartService
{
    private const string GuestUserIdPrefix = "guest:";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public Task<CartDto> GetCart(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        return IsGuestUserId(userId)
            ? GetGuestCartAsync(userId, cancellationToken)
            : GetDbCartAsync(userId, cancellationToken);
    }

    public async Task<CartDto> AddItem(
        string userId,
        int productId,
        int qty,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        if (qty < 1)
        {
            throw new InvalidOperationException("Quantity must be at least 1.");
        }

        var product = await FindCartableProductAsync(productId, cancellationToken);
        if (product is null)
        {
            throw new InvalidOperationException("Product is not available.");
        }

        if (product.StockQuantity < 1)
        {
            throw new InvalidOperationException("Product is out of stock.");
        }

        if (qty > product.StockQuantity)
        {
            throw new InvalidOperationException(
                $"Only {product.StockQuantity} unit(s) in stock.");
        }

        if (IsGuestUserId(userId))
        {
            var lines = await ReadGuestLinesAsync(userId, cancellationToken);
            var line = lines.FirstOrDefault(l => l.ProductId == productId);
            if (line is not null)
            {
                var combined = line.Quantity + qty;
                if (combined > product.StockQuantity)
                {
                    throw new InvalidOperationException(
                        $"Only {product.StockQuantity} unit(s) in stock. Your cart already has {line.Quantity}.");
                }

                line.Quantity = combined;
            }
            else
            {
                lines.Add(new GuestCartLine { ProductId = productId, Quantity = qty });
            }

            await WriteGuestLinesAsync(userId, lines, cancellationToken);
            return await BuildGuestCartDtoAsync(lines, cancellationToken);
        }

        var existing = await db.CartItems
            .FirstOrDefaultAsync(
                c => c.IdentityUserId == userId && c.ProductId == productId,
                cancellationToken);

        if (existing is not null)
        {
            var combined = existing.Quantity + qty;
            if (combined > product.StockQuantity)
            {
                throw new InvalidOperationException(
                    $"Only {product.StockQuantity} unit(s) in stock. Your cart already has {existing.Quantity}.");
            }

            existing.Quantity = combined;
            existing.UpdatedAtUtc = DateTime.UtcNow;
        }
        else
        {
            await db.CartItems.AddAsync(
                new CartItem
                {
                    IdentityUserId = userId,
                    ProductId = productId,
                    Quantity = qty,
                    UpdatedAtUtc = DateTime.UtcNow
                },
                cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        return await GetDbCartAsync(userId, cancellationToken);
    }

    public async Task<CartDto> UpdateQty(
        string userId,
        int itemId,
        int qty,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        if (IsGuestUserId(userId))
        {
            var lines = await ReadGuestLinesAsync(userId, cancellationToken);
            var line = lines.FirstOrDefault(l => l.ProductId == itemId);
            if (line is null)
            {
                throw new InvalidOperationException("Cart line not found.");
            }

            if (qty < 1)
            {
                lines.Remove(line);
            }
            else
            {
                var product = await FindCartableProductAsync(itemId, cancellationToken);
                if (product is null)
                {
                    throw new InvalidOperationException("Product is not available.");
                }

                if (qty > product.StockQuantity)
                {
                    throw new InvalidOperationException(
                        $"Only {product.StockQuantity} unit(s) in stock.");
                }

                line.Quantity = qty;
            }

            await WriteGuestLinesAsync(userId, lines, cancellationToken);
            return await BuildGuestCartDtoAsync(lines, cancellationToken);
        }

        var existing = await db.CartItems
            .Include(c => c.Product)
            .FirstOrDefaultAsync(
                c => c.IdentityUserId == userId && c.Id == itemId,
                cancellationToken);

        if (existing is null)
        {
            throw new InvalidOperationException("Cart line not found.");
        }

        if (qty < 1)
        {
            db.CartItems.Remove(existing);
        }
        else
        {
            var maxQty = existing.Product!.StockQuantity;
            if (qty > maxQty)
            {
                throw new InvalidOperationException(
                    $"Only {maxQty} unit(s) in stock.");
            }

            existing.Quantity = qty;
            existing.UpdatedAtUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
        return await GetDbCartAsync(userId, cancellationToken);
    }

    public async Task<CartDto> RemoveItem(
        string userId,
        int itemId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        if (IsGuestUserId(userId))
        {
            var lines = await ReadGuestLinesAsync(userId, cancellationToken);
            var line = lines.FirstOrDefault(l => l.ProductId == itemId);
            if (line is null)
            {
                throw new InvalidOperationException("Cart line not found.");
            }

            lines.Remove(line);
            await WriteGuestLinesAsync(userId, lines, cancellationToken);
            return await BuildGuestCartDtoAsync(lines, cancellationToken);
        }

        var existing = await db.CartItems
            .FirstOrDefaultAsync(
                c => c.IdentityUserId == userId && c.Id == itemId,
                cancellationToken);

        if (existing is null)
        {
            throw new InvalidOperationException("Cart line not found.");
        }

        db.CartItems.Remove(existing);
        await db.SaveChangesAsync(cancellationToken);
        return await GetDbCartAsync(userId, cancellationToken);
    }

    public async Task<CartDto> ClearCart(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        if (IsGuestUserId(userId))
        {
            await cache.RemoveAsync(GuestCacheKey(userId), cancellationToken);
            return new CartDto
            {
                Lines = Array.Empty<CartLineDto>(),
                SubTotal = 0,
                DiscountTotal = 0,
                TotalItemCount = 0,
                IsAuthenticated = false
            };
        }

        var items = await db.CartItems
            .Where(c => c.IdentityUserId == userId)
            .ToListAsync(cancellationToken);

        if (items.Count > 0)
        {
            db.CartItems.RemoveRange(items);
            await db.SaveChangesAsync(cancellationToken);
        }

        return await GetDbCartAsync(userId, cancellationToken);
    }

    private static bool IsGuestUserId(string userId) =>
        userId.StartsWith(GuestUserIdPrefix, StringComparison.Ordinal);

    private static string GuestCacheKey(string userId) => $"cart:{userId}";

    private async Task<List<GuestCartLine>> ReadGuestLinesAsync(string userId, CancellationToken cancellationToken)
    {
        var bytes = await cache.GetAsync(GuestCacheKey(userId), cancellationToken);
        if (bytes is null || bytes.Length == 0)
        {
            return new List<GuestCartLine>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<GuestCartLine>>(bytes, JsonOptions) ?? new List<GuestCartLine>();
        }
        catch
        {
            return new List<GuestCartLine>();
        }
    }

    private Task WriteGuestLinesAsync(string userId, List<GuestCartLine> lines, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(lines, JsonOptions);
        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromDays(14)
        };
        return cache.SetAsync(GuestCacheKey(userId), payload, options, cancellationToken);
    }

    private async Task<CartDto> GetGuestCartAsync(string userId, CancellationToken cancellationToken)
    {
        var lines = await ReadGuestLinesAsync(userId, cancellationToken);
        return await BuildGuestCartDtoAsync(lines, cancellationToken);
    }

    private async Task<CartDto> GetDbCartAsync(string identityUserId, CancellationToken cancellationToken)
    {
        var items = await db.CartItems
            .AsNoTracking()
            .Include(c => c.Product)
            .Where(c => c.IdentityUserId == identityUserId)
            .ToListAsync(cancellationToken);

        var lines = new List<CartLineDto>();
        decimal subTotal = 0;
        decimal discountTotal = 0;
        var totalQty = 0;

        foreach (var item in items)
        {
            if (item.Product is null || !item.Product.IsPublished || item.Product.IsDeleted)
            {
                continue;
            }

            var p = item.Product;
            var qty = Math.Min(item.Quantity, p.StockQuantity);
            if (qty < 1)
            {
                continue;
            }

            var pricing = ComputeLinePricing(p, qty);
            subTotal += pricing.LineTotal;
            discountTotal += pricing.LineDiscount;
            totalQty += qty;
            lines.Add(
                new CartLineDto
                {
                    CartItemId = item.Id,
                    ProductId = item.ProductId,
                    Name = p.Name,
                    ImageUrl = p.ImageUrl,
                    UnitPrice = pricing.UnitPrice,
                    CompareAtPrice = pricing.CompareAtDisplay,
                    Quantity = qty,
                    LineTotal = pricing.LineTotal,
                    LineDiscount = pricing.LineDiscount
                });
        }

        return new CartDto
        {
            Lines = lines,
            SubTotal = subTotal,
            DiscountTotal = discountTotal,
            TotalItemCount = totalQty,
            IsAuthenticated = true
        };
    }

    private async Task<CartDto> BuildGuestCartDtoAsync(
        List<GuestCartLine> guestLines,
        CancellationToken cancellationToken)
    {
        if (guestLines.Count == 0)
        {
            return new CartDto
            {
                Lines = Array.Empty<CartLineDto>(),
                SubTotal = 0,
                DiscountTotal = 0,
                TotalItemCount = 0,
                IsAuthenticated = false
            };
        }

        var ids = guestLines.Select(l => l.ProductId).Distinct().ToList();
        var products = await db.Products
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id) && p.IsPublished)
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var lines = new List<CartLineDto>();
        decimal subTotal = 0;
        decimal discountTotal = 0;
        var totalQty = 0;

        foreach (var gl in guestLines)
        {
            if (!products.TryGetValue(gl.ProductId, out var product))
            {
                continue;
            }

            if (gl.Quantity > product.StockQuantity)
            {
                continue;
            }

            var q = gl.Quantity;
            if (q < 1)
            {
                continue;
            }

            var pricing = ComputeLinePricing(product, q);
            subTotal += pricing.LineTotal;
            discountTotal += pricing.LineDiscount;
            totalQty += q;
            lines.Add(
                new CartLineDto
                {
                    CartItemId = 0,
                    ProductId = gl.ProductId,
                    Name = product.Name,
                    ImageUrl = product.ImageUrl,
                    UnitPrice = pricing.UnitPrice,
                    CompareAtPrice = pricing.CompareAtDisplay,
                    Quantity = q,
                    LineTotal = pricing.LineTotal,
                    LineDiscount = pricing.LineDiscount
                });
        }

        return new CartDto
        {
            Lines = lines,
            SubTotal = subTotal,
            DiscountTotal = discountTotal,
            TotalItemCount = totalQty,
            IsAuthenticated = false
        };
    }

    private static CartLinePricing ComputeLinePricing(Product p, int quantity)
    {
        var unit = p.Price;
        decimal lineTotal = unit * quantity;
        decimal lineDiscount = 0;
        decimal? compareDisplay = null;

        if (p.CompareAtPrice is { } compare && compare > unit)
        {
            compareDisplay = compare;
            lineDiscount = (compare - unit) * quantity;
        }

        return new CartLinePricing(unit, compareDisplay, lineTotal, lineDiscount);
    }

    private readonly record struct CartLinePricing(
        decimal UnitPrice,
        decimal? CompareAtDisplay,
        decimal LineTotal,
        decimal LineDiscount);

    private async Task<Product?> FindCartableProductAsync(int productId, CancellationToken cancellationToken)
    {
        return await db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.Id == productId && p.IsPublished,
                cancellationToken);
    }
}
