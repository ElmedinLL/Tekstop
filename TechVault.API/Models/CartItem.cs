namespace TechVault.API.Models;

public class CartItem
{
    public int Id { get; set; }
    public string IdentityUserId { get; set; } = null!;
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public Product Product { get; set; } = null!;
}
