namespace TechVault.API.Orders;

public sealed class CreateOrderDto
{
    public int AddressId { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string? CouponCode { get; set; }

    /// <summary>standard or express</summary>
    public string ShippingMethod { get; set; } = "standard";

    public List<CreateOrderCartItemDto> CartItems { get; set; } = new();
}
