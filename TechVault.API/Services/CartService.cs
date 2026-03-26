using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TechVault.API.Carts;
using TechVault.API.Data;
using TechVault.API.Models;

namespace TechVault.API.Services;

public sealed class CartService(
    ApplicationDbContext db,
    IHttpContextAccessor httpContextAccessor) : ICartService
{
    private const string GuestSessionKey = "cart_guest_v1";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private HttpContext Http =>
        httpContextAccessor.HttpContext ?? throw new InvalidOperationException();

    private ISession Session => Http.Session;

    public async Task<CartDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var identityUserId = ResolveIdentityUserId();
        if (identityUserId is not null)
        {
            return await GetDbCartAsync(identityUserId, cancellationToken);
        }

        await Session.LoadAsync(cancellationToken);
        var guestLines = ReadGuestLines();
        return await BuildGuestCartDtoAsync(guestLines, cancellationToken);
    }

    public async Task<CartDto> AddAsync(AddToCartDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Quantity < 1)
        {
            throw new InvalidOperationException("Quantity must be at least 1.");
        }

        var identityUserId = ResolveIdentityUserId();
        if (identityUserId is not null)
        {
            var product = await FindCartableProductAsync(dto.ProductId, cancellationToken);
            if (product is null)
            {
                throw new InvalidOperationException("Product is not available.");
            }

            var qty = Math.Min(dto.Quantity, product.StockQuantity);
            if (qty < 1)
            {
                throw new InvalidOperationException("Product is out of stock.");
            }

            var existing = await db.CartItems
                .FirstOrDefaultAsync(
                    c => c.IdentityUserId == identityUserId && c.ProductId == dto.ProductId,
                    cancellationToken);

            if (existing is not null)
            {
                var combined = Math.Min(existing.Quantity + qty, product.StockQuantity);
                existing.Quantity = combined;
                existing.UpdatedAtUtc = DateTime.UtcNow;
            }
            else
            {
                await db.CartItems.AddAsync(
                    new CartItem
                    {
                        IdentityUserId = identityUserId,
                        ProductId = dto.ProductId,
                        Quantity = qty,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    cancellationToken);
            }

            await db.SaveChangesAsync(cancellationToken);
            return await GetDbCartAsync(identityUserId, cancellationToken);
        }

        await Session.LoadAsync(cancellationToken);
        var lines = ReadGuestLines();
        var p = await FindCartableProductAsync(dto.ProductId, cancellationToken);
        if (p is null)
        {
            throw new InvalidOperationException("Product is not available.");
        }

        var addQty = Math.Min(dto.Quantity, p.StockQuantity);
        if (addQty < 1)
        {
            throw new InvalidOperationException("Product is out of stock.");
        }

        var line = lines.FirstOrDefault(l => l.ProductId == dto.ProductId);
        if (line is not null)
        {
            line.Quantity = Math.Min(line.Quantity + addQty, p.StockQuantity);
        }
        else
        {
            lines.Add(new GuestCartLine { ProductId = dto.ProductId, Quantity = addQty });
        }

        WriteGuestLines(lines);
        await Session.CommitAsync(cancellationToken);
        return await BuildGuestCartDtoAsync(lines, cancellationToken);
    }

    public async Task<CartDto> UpdateAsync(UpdateCartDto dto, CancellationToken cancellationToken = default)
    {
        var identityUserId = ResolveIdentityUserId();
        if (identityUserId is not null)
        {
            var existing = await db.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(
                    c => c.IdentityUserId == identityUserId && c.ProductId == dto.ProductId,
                    cancellationToken);

            if (existing is null)
            {
                throw new InvalidOperationException("Cart line not found.");
            }

            if (dto.Quantity < 1)
            {
                db.CartItems.Remove(existing);
            }
            else
            {
                var maxQty = existing.Product.StockQuantity;
                existing.Quantity = Math.Min(dto.Quantity, maxQty);
                existing.UpdatedAtUtc = DateTime.UtcNow;
            }

            await db.SaveChangesAsync(cancellationToken);
            return await GetDbCartAsync(identityUserId, cancellationToken);
        }

        await Session.LoadAsync(cancellationToken);
        var lines = ReadGuestLines();
        var line = lines.FirstOrDefault(l => l.ProductId == dto.ProductId);
        if (line is null)
        {
            throw new InvalidOperationException("Cart line not found.");
        }

        if (dto.Quantity < 1)
        {
            lines.Remove(line);
        }
        else
        {
            var product = await FindCartableProductAsync(dto.ProductId, cancellationToken);
            if (product is null)
            {
                throw new InvalidOperationException("Product is not available.");
            }

            line.Quantity = Math.Min(dto.Quantity, product.StockQuantity);
        }

        WriteGuestLines(lines);
        await Session.CommitAsync(cancellationToken);
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
        var totalQty = 0;

        foreach (var item in items)
        {
            if (item.Product is null || !item.Product.IsPublished || item.Product.IsDeleted)
            {
                continue;
            }

            var lineTotal = item.Product.Price * item.Quantity;
            subTotal += lineTotal;
            totalQty += item.Quantity;
            lines.Add(
                new CartLineDto
                {
                    CartItemId = item.Id,
                    ProductId = item.ProductId,
                    Name = item.Product.Name,
                    ImageUrl = item.Product.ImageUrl,
                    UnitPrice = item.Product.Price,
                    Quantity = item.Quantity,
                    LineTotal = lineTotal
                });
        }

        return new CartDto
        {
            Lines = lines,
            SubTotal = subTotal,
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
        var totalQty = 0;

        foreach (var gl in guestLines)
        {
            if (!products.TryGetValue(gl.ProductId, out var product))
            {
                continue;
            }

            var qty = Math.Min(gl.Quantity, product.StockQuantity);
            if (qty < 1)
            {
                continue;
            }

            var lineTotal = product.Price * qty;
            subTotal += lineTotal;
            totalQty += qty;
            lines.Add(
                new CartLineDto
                {
                    CartItemId = 0,
                    ProductId = gl.ProductId,
                    Name = product.Name,
                    ImageUrl = product.ImageUrl,
                    UnitPrice = product.Price,
                    Quantity = qty,
                    LineTotal = lineTotal
                });
        }

        return new CartDto
        {
            Lines = lines,
            SubTotal = subTotal,
            TotalItemCount = totalQty,
            IsAuthenticated = false
        };
    }

    private async Task<Product?> FindCartableProductAsync(int productId, CancellationToken cancellationToken)
    {
        return await db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.Id == productId && p.IsPublished,
                cancellationToken);
    }

    private List<GuestCartLine> ReadGuestLines()
    {
        var raw = Session.GetString(GuestSessionKey);
        if (string.IsNullOrEmpty(raw))
        {
            return new List<GuestCartLine>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<GuestCartLine>>(raw, JsonOptions) ?? new List<GuestCartLine>();
        }
        catch
        {
            return new List<GuestCartLine>();
        }
    }

    private void WriteGuestLines(List<GuestCartLine> lines)
    {
        Session.SetString(GuestSessionKey, JsonSerializer.Serialize(lines, JsonOptions));
    }

    private string? ResolveIdentityUserId()
    {
        if (Http.User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return Http.User.FindFirstValue("UserId")
            ?? Http.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Http.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
    }
}
