namespace TechVault.API.Models;

/// <summary>
/// Join entity for the many-to-many relationship between <see cref="Product"/> and <see cref="Tag"/>.
/// </summary>
public class ProductTag
{
    public int ProductId { get; set; }
    public int TagId { get; set; }

    public Product Product { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
