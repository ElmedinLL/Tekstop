namespace TechVault.API.Repositories.Products;

public sealed record ProductListFilter(
    int? CategoryId = null,
    /// <summary>When set, restricts to products in this category slug (storefront listing).</summary>
    string? CategorySlug = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? Brand = null,
    bool? InStockOnly = null,
    bool PublishedOnly = true,
    /// <summary>Full-text search on product name and description (MySQL FULLTEXT).</summary>
    string? SearchTerm = null,
    /// <summary>AND filters on specs JSON (e.g. RAM → 16GB).</summary>
    IReadOnlyList<KeyValuePair<string, string>>? SpecFilters = null);
