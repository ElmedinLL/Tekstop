using TechVault.API.Models.Enums;

namespace TechVault.API.Models;

public class Coupon
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public DiscountType DiscountType { get; set; }

    /// <summary>
    /// Percent: 0–100. Fixed: amount in the store currency.
    /// </summary>
    public decimal DiscountValue { get; set; }

    public decimal MinOrderValue { get; set; }

    public DateTime? ExpiresAtUtc { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Maximum redemptions across all customers; null = unlimited.</summary>
    public int? UsageLimit { get; set; }

    public int UsageCount { get; set; }
}
