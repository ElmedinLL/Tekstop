namespace TechVault.API.Carts;

public sealed class CartLineDto
{
    public int CartItemId { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public decimal LineDiscount { get; set; }
}
