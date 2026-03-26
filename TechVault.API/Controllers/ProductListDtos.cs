namespace TechVault.API.Products;

/// <summary>Filters for category product listing; the category slug is taken from the URL path.</summary>
public sealed class ProductCategoryPageQueryParameters
{
    public string? Search { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    /// <summary>price_asc, price_desc, name_asc, name_desc, newest, oldest, stock_desc</summary>
    public string? Sort { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class ProductListQueryParameters
{
    /// <summary>Category id (numeric) or category slug.</summary>
    public string? Category { get; set; }

    public string? Search { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    /// <summary>price_asc, price_desc, name_asc, name_desc, newest, oldest</summary>
    public string? Sort { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class ProductListItemDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public decimal Price { get; init; }
    public decimal? CompareAtPrice { get; init; }
    public string? ImageUrl { get; init; }
    public string? Brand { get; init; }
    public string CategoryName { get; init; } = null!;
    public string CategorySlug { get; init; } = null!;
    public int StockQuantity { get; init; }
    public bool IsPublished { get; init; }
}

public sealed class PagedProductsResponse
{
    public IReadOnlyList<ProductListItemDto> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}
