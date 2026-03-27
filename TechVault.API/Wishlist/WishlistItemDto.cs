namespace TechVault.API.Wishlist;

public sealed class WishlistItemDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime AddedAtUtc { get; set; }
}
