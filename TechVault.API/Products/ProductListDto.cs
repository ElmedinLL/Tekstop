namespace TechVault.API.Products;

public sealed class ProductListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public decimal? AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public IReadOnlyList<string> Images { get; set; } = Array.Empty<string>();
    public ProductCategoryDto Category { get; set; } = null!;
    public IReadOnlyDictionary<string, string> Specs { get; set; } =
        new Dictionary<string, string>();
}
