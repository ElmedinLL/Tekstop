using Microsoft.EntityFrameworkCore;
using TechVault.API.Coupons;
using TechVault.API.Data;
using TechVault.API.Models.Enums;

namespace TechVault.API.Services;

public sealed class CouponValidationService(ApplicationDbContext db) : ICouponValidationService
{
    public async Task<ValidateCouponResponseDto> ValidateAsync(
        string code,
        decimal orderSubtotal,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Invalid("Coupon code is required.");
        }

        if (orderSubtotal < 0)
        {
            return Invalid("Order subtotal is invalid.");
        }

        var normalized = code.Trim().ToUpperInvariant();
        var coupon = await db.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == normalized, cancellationToken);

        if (coupon is null)
        {
            return Invalid("This coupon code is not recognized.");
        }

        if (!coupon.IsActive)
        {
            return Invalid("This coupon is no longer active.");
        }

        if (coupon.ExpiresAtUtc is { } exp && exp < DateTime.UtcNow)
        {
            return Invalid("This coupon has expired.");
        }

        if (orderSubtotal < coupon.MinOrderValue)
        {
            return Invalid(
                $"This coupon requires a minimum order of {coupon.MinOrderValue:0.00}.");
        }

        if (coupon.UsageLimit is { } cap && cap > 0 && coupon.UsageCount >= cap)
        {
            return Invalid("This coupon has reached its usage limit.");
        }

        var discount = coupon.DiscountType switch
        {
            DiscountType.Percent => Math.Round(orderSubtotal * (coupon.DiscountValue / 100m), 2, MidpointRounding.AwayFromZero),
            DiscountType.Fixed => Math.Min(coupon.DiscountValue, orderSubtotal),
            _ => 0m
        };

        if (discount <= 0)
        {
            return Invalid("This coupon does not apply to your order.");
        }

        return new ValidateCouponResponseDto
        {
            IsValid = true,
            Message = "Coupon applied.",
            Code = coupon.Code,
            DiscountType = coupon.DiscountType,
            DiscountAmount = discount
        };
    }

    private static ValidateCouponResponseDto Invalid(string message) =>
        new()
        {
            IsValid = false,
            Message = message,
            DiscountAmount = 0
        };
}
