namespace TechVault.API.Products;

public sealed class CreateProductDto
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public string? Brand { get; set; }
    public bool IsPublished { get; set; } = true;
    public List<string> Images { get; set; } = new();
    public Dictionary<string, string> Specs { get; set; } = new();
}
