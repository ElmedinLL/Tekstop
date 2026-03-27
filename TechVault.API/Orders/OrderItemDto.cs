namespace TechVault.API.Orders;

public sealed class OrderItemDto
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string ProductSku { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }
}
