namespace TechVault.API.Carts;

public sealed class AddToCartDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}
