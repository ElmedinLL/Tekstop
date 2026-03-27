namespace TechVault.API.Products;

public sealed class ProductImageUploadResponse
{
    public int Id { get; set; }
    public string Url { get; set; } = null!;
    public int SortOrder { get; set; }
}
