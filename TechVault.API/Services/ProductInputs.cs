namespace TechVault.API.Services;

public sealed class CreateProductInput
{
    public int CategoryId { get; init; }
    public string Name { get; init; } = null!;
    public string Sku { get; init; } = null!;
    public decimal Price { get; init; }
    public decimal? CompareAtPrice { get; init; }
    public int StockQuantity { get; init; }
    public string? ShortDescription { get; init; }
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public string? Brand { get; init; }
    public bool IsPublished { get; init; }

    /// <summary>When null or whitespace, the slug is generated from <see cref="Name"/>.</summary>
    public string? Slug { get; init; }
}

public sealed class UpdateProductInput
{
    public int CategoryId { get; init; }
    public string Name { get; init; } = null!;
    public string Sku { get; init; } = null!;
    public decimal Price { get; init; }
    public decimal? CompareAtPrice { get; init; }
    public int StockQuantity { get; init; }
    public string? ShortDescription { get; init; }
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public string? Brand { get; init; }
    public bool IsPublished { get; init; }

    /// <summary>When null or whitespace, the slug is regenerated from <see cref="Name"/>.</summary>
    public string? Slug { get; init; }
}
