using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechVault.API.Data;
using TechVault.API.Models;
using TechVault.API.Models.Enums;
using TechVault.API.Reviews;
using TechVault.API.Services;

namespace TechVault.API.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/products/{productId:int}/reviews")]
public sealed class ReviewController(
    ApplicationDbContext db,
    IDomainUserService domainUserService,
    IProductReviewStatsService productReviewStatsService) : ControllerBase
{
    /// <summary>Lists approved reviews for a published product.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ProductReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProductReviewDto>>> List(
        int productId,
        CancellationToken cancellationToken)
    {
        var productExists = await db.Products
            .AsNoTracking()
            .AnyAsync(p => p.Id == productId && p.IsPublished, cancellationToken);

        if (!productExists)
        {
            return NotFound();
        }

        var rows = await (
            from r in db.Reviews.AsNoTracking()
            join u in db.Users.AsNoTracking() on r.UserId equals u.Id
            where r.ProductId == productId && r.IsApproved
            orderby r.CreatedAtUtc descending
            select new { r.Id, r.Rating, r.Comment, r.CreatedAtUtc, u.FirstName, u.LastName }
        ).ToListAsync(cancellationToken);

        var list = rows.ConvertAll(x => new ProductReviewDto
        {
            Id = x.Id,
            Rating = x.Rating,
            Comment = x.Comment,
            CreatedAtUtc = x.CreatedAtUtc,
            AuthorDisplayName = FormatAuthorDisplayName(x.FirstName, x.LastName)
        });

        return Ok(list);
    }

    /// <summary>
    /// Returns whether the current user has purchased this product (non-cancelled / non-refunded order)
    /// and their review row if they submitted one (including pending approval).
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(MyReviewStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MyReviewStatusDto>> GetMyStatus(int productId, CancellationToken cancellationToken)
    {
        var identityUserId = ResolveIdentityUserId();
        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        var domainUserId = await domainUserService.GetOrCreateDomainUserIdAsync(identityUserId, cancellationToken);

        var productExists = await db.Products
            .AsNoTracking()
            .AnyAsync(p => p.Id == productId && p.IsPublished, cancellationToken);

        if (!productExists)
        {
            return NotFound();
        }

        var purchased = await UserHasPurchasedProductAsync(domainUserId, productId, cancellationToken);

        var reviewRow = await (
            from r in db.Reviews.AsNoTracking()
            join u in db.Users.AsNoTracking() on r.UserId equals u.Id
            where r.UserId == domainUserId && r.ProductId == productId
            select new { r.Id, r.Rating, r.Comment, r.CreatedAtUtc, u.FirstName, u.LastName }
        ).FirstOrDefaultAsync(cancellationToken);

        ProductReviewDto? review = null;
        if (reviewRow != null)
        {
            review = new ProductReviewDto
            {
                Id = reviewRow.Id,
                Rating = reviewRow.Rating,
                Comment = reviewRow.Comment,
                CreatedAtUtc = reviewRow.CreatedAtUtc,
                AuthorDisplayName = FormatAuthorDisplayName(reviewRow.FirstName, reviewRow.LastName)
            };
        }

        return Ok(new MyReviewStatusDto { Purchased = purchased, Review = review });
    }

    /// <summary>Adds a review if the current user bought this product in a non-cancelled order.</summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ProductReviewDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductReviewDto>> Create(
        int productId,
        [FromBody] CreateProductReviewDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var identityUserId = ResolveIdentityUserId();
        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        var domainUserId = await domainUserService.GetOrCreateDomainUserIdAsync(identityUserId, cancellationToken);

        var productExists = await db.Products
            .AsNoTracking()
            .AnyAsync(p => p.Id == productId && p.IsPublished, cancellationToken);

        if (!productExists)
        {
            return NotFound();
        }

        if (!await UserHasPurchasedProductAsync(domainUserId, productId, cancellationToken))
        {
            return BadRequest("You can only review products you have purchased.");
        }

        if (await db.Reviews.AnyAsync(r => r.UserId == domainUserId && r.ProductId == productId, cancellationToken))
        {
            return Conflict("You have already reviewed this product.");
        }

        var now = DateTime.UtcNow;
        var entity = new Review
        {
            UserId = domainUserId,
            ProductId = productId,
            Rating = (byte)dto.Rating,
            Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim(),
            Title = string.IsNullOrWhiteSpace(dto.Title) ? null : dto.Title.Trim(),
            IsApproved = true,
            CreatedAtUtc = now
        };

        db.Reviews.Add(entity);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Conflict("You have already reviewed this product.");
        }

        await productReviewStatsService.RecalculateForProductAsync(productId, cancellationToken);

        var user = await db.Users.AsNoTracking()
            .Where(u => u.Id == domainUserId)
            .Select(u => new { u.FirstName, u.LastName })
            .FirstAsync(cancellationToken);

        var response = new ProductReviewDto
        {
            Id = entity.Id,
            Rating = entity.Rating,
            Comment = entity.Comment,
            CreatedAtUtc = entity.CreatedAtUtc,
            AuthorDisplayName = FormatAuthorDisplayName(user.FirstName, user.LastName)
        };

        return CreatedAtAction(
            nameof(List),
            new { version = HttpContext.GetRequestedApiVersion()!.ToString(), productId },
            response);
    }

    /// <summary>Deletes the current user&apos;s review for this product.</summary>
    [HttpDelete]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOwn(int productId, CancellationToken cancellationToken)
    {
        var identityUserId = ResolveIdentityUserId();
        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        var domainUserId = await domainUserService.GetOrCreateDomainUserIdAsync(identityUserId, cancellationToken);

        var review = await db.Reviews
            .FirstOrDefaultAsync(r => r.UserId == domainUserId && r.ProductId == productId, cancellationToken);

        if (review is null)
        {
            return NotFound();
        }

        db.Reviews.Remove(review);
        await db.SaveChangesAsync(cancellationToken);
        await productReviewStatsService.RecalculateForProductAsync(productId, cancellationToken);
        return NoContent();
    }

    private async Task<bool> UserHasPurchasedProductAsync(
        int domainUserId,
        int productId,
        CancellationToken cancellationToken)
    {
        return await (
            from o in db.Orders.AsNoTracking()
            join oi in db.OrderItems.AsNoTracking() on o.Id equals oi.OrderId
            where o.UserId == domainUserId
                  && oi.ProductId == productId
                  && o.Status != OrderStatus.Cancelled
                  && o.Status != OrderStatus.Refunded
            select o.Id
        ).AnyAsync(cancellationToken);
    }

    private static string FormatAuthorDisplayName(string firstName, string lastName)
    {
        var first = string.IsNullOrWhiteSpace(firstName) ? "Customer" : firstName.Trim();
        var last = string.IsNullOrWhiteSpace(lastName) ? string.Empty : lastName.Trim();
        if (last.Length == 0)
        {
            return first;
        }

        return $"{first} {last[0]}.";
    }

    private string? ResolveIdentityUserId()
        => User.FindFirstValue("UserId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
}
