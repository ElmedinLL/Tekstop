using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Asp.Versioning.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechVault.API.Data;
using TechVault.API.Models;
using TechVault.API.Services;
using TechVault.API.Wishlist;

namespace TechVault.API.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/wishlist")]
[Authorize]
public sealed class WishlistController(
    ApplicationDbContext db,
    IDomainUserService domainUserService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WishlistItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<WishlistItemDto>>> List(CancellationToken cancellationToken)
    {
        var identityUserId = ResolveIdentityUserId();
        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        var domainUserId = await domainUserService.GetOrCreateDomainUserIdAsync(identityUserId, cancellationToken);

        var items = await (
            from w in db.WishlistItems.AsNoTracking()
            where w.UserId == domainUserId
            join p in db.Products.AsNoTracking() on w.ProductId equals p.Id
            where p.IsPublished
            orderby w.AddedAtUtc descending
            select new WishlistItemDto
            {
                ProductId = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                AddedAtUtc = w.AddedAtUtc
            }).ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpPost("{productId:int}")]
    [ProducesResponseType(typeof(WishlistItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WishlistItemDto>> Add(int productId, CancellationToken cancellationToken)
    {
        var identityUserId = ResolveIdentityUserId();
        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        var domainUserId = await domainUserService.GetOrCreateDomainUserIdAsync(identityUserId, cancellationToken);

        var product = await db.Products
            .AsNoTracking()
            .Where(p => p.Id == productId && p.IsPublished)
            .Select(p => new { p.Id, p.Name, p.Slug, p.Price, p.ImageUrl })
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        var existing = await db.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == domainUserId && w.ProductId == productId, cancellationToken);

        if (existing is not null)
        {
            return Ok(new WishlistItemDto
            {
                ProductId = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                AddedAtUtc = existing.AddedAtUtc
            });
        }

        var now = DateTime.UtcNow;
        var row = new WishlistItem
        {
            UserId = domainUserId,
            ProductId = productId,
            AddedAtUtc = now
        };

        db.WishlistItems.Add(row);
        await db.SaveChangesAsync(cancellationToken);

        return Ok(new WishlistItemDto
        {
            ProductId = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
            AddedAtUtc = now
        });
    }

    [HttpDelete("{productId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(int productId, CancellationToken cancellationToken)
    {
        var identityUserId = ResolveIdentityUserId();
        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        var domainUserId = await domainUserService.GetOrCreateDomainUserIdAsync(identityUserId, cancellationToken);

        var row = await db.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == domainUserId && w.ProductId == productId, cancellationToken);

        if (row is null)
        {
            return NotFound();
        }

        db.WishlistItems.Remove(row);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private string? ResolveIdentityUserId()
        => User.FindFirstValue("UserId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
}
