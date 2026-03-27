namespace TechVault.API.Carts;

public sealed class CartDto
{
    public IReadOnlyList<CartLineDto> Lines { get; set; } = Array.Empty<CartLineDto>();
    public decimal SubTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public int TotalItemCount { get; set; }
    public bool IsAuthenticated { get; set; }
}
