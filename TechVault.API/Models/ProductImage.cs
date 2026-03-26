namespace TechVault.API.Models;

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Url { get; set; } = null!;
    public int SortOrder { get; set; }

    public Product Product { get; set; } = null!;
}
