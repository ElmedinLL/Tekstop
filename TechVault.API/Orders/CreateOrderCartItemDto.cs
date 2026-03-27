namespace TechVault.API.Orders;

public sealed class CreateOrderCartItemDto
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }
}
