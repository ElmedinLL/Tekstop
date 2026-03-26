namespace TechVault.API.Products;

public sealed class ProductCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
}
