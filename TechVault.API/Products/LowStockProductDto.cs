namespace TechVault.API.Products;

/// <summary>Admin view of a product that is below the configured low-stock threshold.</summary>
public sealed class LowStockProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string Sku { get; init; } = null!;
    public int StockQuantity { get; init; }
    public bool IsPublished { get; init; }
    public string CategoryName { get; init; } = null!;
}
