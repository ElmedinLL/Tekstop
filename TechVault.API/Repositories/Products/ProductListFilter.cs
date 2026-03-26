namespace TechVault.API.Repositories.Products;

public sealed record ProductListFilter(
    int? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? Brand = null,
    bool? InStockOnly = null,
    bool PublishedOnly = true);
