using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechVault.API.Coupons;
using TechVault.API.Data;
using TechVault.API.Models;
using TechVault.API.Models.Enums;

namespace TechVault.API.Controllers;

[ApiController]
[Route("api/admin/coupons")]
[Authorize(Roles = "Admin")]
public sealed class AdminCouponsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AdminCouponDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AdminCouponDto>>> List(CancellationToken cancellationToken)
    {
        var items = await db.Coupons.AsNoTracking()
            .OrderBy(c => c.Code)
            .Select(c => new AdminCouponDto
            {
                Id = c.Id,
                Code = c.Code,
                DiscountType = c.DiscountType,
                DiscountValue = c.DiscountValue,
                MinOrderValue = c.MinOrderValue,
                ExpiresAtUtc = c.ExpiresAtUtc,
                UsageLimit = c.UsageLimit,
                UsageCount = c.UsageCount,
                IsActive = c.IsActive,
            })
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AdminCouponDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminCouponDto>> Create(
        [FromBody] CreateAdminCouponDto dto,
        CancellationToken cancellationToken)
    {
        var err = ValidateCouponPayload(dto.DiscountType, dto.DiscountValue, dto.MinOrderValue, dto.UsageLimit);
        if (err is not null)
        {
            return BadRequest(err);
        }

        var code = dto.Code.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(code))
        {
            return BadRequest("Code is required.");
        }

        if (await db.Coupons.AnyAsync(c => c.Code == code, cancellationToken))
        {
            return Conflict("A coupon with this code already exists.");
        }

        var entity = new Coupon
        {
            Code = code,
            DiscountType = dto.DiscountType,
            DiscountValue = dto.DiscountValue,
            MinOrderValue = dto.MinOrderValue,
            ExpiresAtUtc = dto.ExpiresAtUtc,
            UsageLimit = dto.UsageLimit is > 0 ? dto.UsageLimit : null,
            UsageCount = 0,
            IsActive = dto.IsActive,
        };

        db.Coupons.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return StatusCode(StatusCodes.Status201Created, Map(entity));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(AdminCouponDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminCouponDto>> Update(
        int id,
        [FromBody] UpdateAdminCouponDto dto,
        CancellationToken cancellationToken)
    {
        var err = ValidateCouponPayload(dto.DiscountType, dto.DiscountValue, dto.MinOrderValue, dto.UsageLimit);
        if (err is not null)
        {
            return BadRequest(err);
        }

        var code = dto.Code.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(code))
        {
            return BadRequest("Code is required.");
        }

        var entity = await db.Coupons.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        if (await db.Coupons.AnyAsync(c => c.Id != id && c.Code == code, cancellationToken))
        {
            return Conflict("A coupon with this code already exists.");
        }

        entity.Code = code;
        entity.DiscountType = dto.DiscountType;
        entity.DiscountValue = dto.DiscountValue;
        entity.MinOrderValue = dto.MinOrderValue;
        entity.ExpiresAtUtc = dto.ExpiresAtUtc;
        entity.UsageLimit = dto.UsageLimit is > 0 ? dto.UsageLimit : null;
        entity.IsActive = dto.IsActive;

        await db.SaveChangesAsync(cancellationToken);

        return Ok(Map(entity));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var entity = await db.Coupons.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        db.Coupons.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static AdminCouponDto Map(Coupon c) =>
        new()
        {
            Id = c.Id,
            Code = c.Code,
            DiscountType = c.DiscountType,
            DiscountValue = c.DiscountValue,
            MinOrderValue = c.MinOrderValue,
            ExpiresAtUtc = c.ExpiresAtUtc,
            UsageLimit = c.UsageLimit,
            UsageCount = c.UsageCount,
            IsActive = c.IsActive,
        };

    private static string? ValidateCouponPayload(
        DiscountType type,
        decimal discountValue,
        decimal minOrder,
        int? usageLimit)
    {
        if (minOrder < 0)
        {
            return "Minimum order cannot be negative.";
        }

        if (usageLimit is < 0)
        {
            return "Usage limit cannot be negative.";
        }

        return type switch
        {
            DiscountType.Percent when discountValue <= 0 || discountValue > 100 =>
                "Percent discount must be between 0 and 100.",
            DiscountType.Fixed when discountValue <= 0 =>
                "Fixed discount must be greater than 0.",
            _ => null
        };
    }
}
