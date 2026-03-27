namespace TechVault.API.Coupons;

public sealed class ValidateCouponDto
{
    public string Code { get; set; } = null!;

    /// <summary>Cart subtotal before shipping and tax.</summary>
    public decimal OrderSubtotal { get; set; }
}
