namespace TechVault.API.Categories;

public sealed class CategoryListItemDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public int? ParentCategoryId { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public int ProductCount { get; init; }
}

public sealed class CategoryDetailDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public int? ParentCategoryId { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public int ProductCount { get; init; }
}

public sealed class CreateCategoryDto
{
    public string Name { get; set; } = null!;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? ParentCategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>Full replacement body for <c>PUT /api/categories/{slug}</c>.</summary>
public sealed class UpdateCategoryDto
{
    public string Name { get; set; } = null!;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? ParentCategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class CategoryImageUploadResponse
{
    public string Url { get; set; } = null!;
}
