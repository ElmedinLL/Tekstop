using TechVault.API.Models.Enums;

namespace TechVault.API.Coupons;

public sealed class AdminCouponDto
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public DiscountType DiscountType { get; set; }

    public decimal DiscountValue { get; set; }

    public decimal MinOrderValue { get; set; }

    public DateTime? ExpiresAtUtc { get; set; }

    public int? UsageLimit { get; set; }

    public int UsageCount { get; set; }

    public bool IsActive { get; set; }
}

public class CreateAdminCouponDto
{
    public string Code { get; set; } = null!;

    public DiscountType DiscountType { get; set; }

    public decimal DiscountValue { get; set; }

    public decimal MinOrderValue { get; set; }

    public DateTime? ExpiresAtUtc { get; set; }

    public int? UsageLimit { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateAdminCouponDto : CreateAdminCouponDto
{
}
