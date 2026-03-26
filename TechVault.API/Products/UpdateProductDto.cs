namespace TechVault.API.Products;

public sealed class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? Sku { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? Stock { get; set; }
    public int? CategoryId { get; set; }
    public List<string>? Images { get; set; }
    public Dictionary<string, string>? Specs { get; set; }
}
