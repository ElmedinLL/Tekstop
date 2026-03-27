namespace TechVault.API.Models;

/// <summary>Customer wishlist entry: one row per user and product.</summary>
public class WishlistItem
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ProductId { get; set; }

    public DateTime AddedAtUtc { get; set; }

    public User User { get; set; } = null!;

    public Product Product { get; set; } = null!;
}
