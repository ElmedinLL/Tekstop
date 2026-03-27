using TechVault.API.Models.Enums;

namespace TechVault.API.Coupons;

public sealed class ValidateCouponResponseDto
{
    public bool IsValid { get; set; }

    public string Message { get; set; } = null!;

    public string? Code { get; set; }

    public DiscountType? DiscountType { get; set; }

    public decimal DiscountAmount { get; set; }
}
