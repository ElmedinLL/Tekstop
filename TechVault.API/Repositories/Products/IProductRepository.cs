using TechVault.API.Models;
using TechVault.API.Repositories;

namespace TechVault.API.Repositories.Products;

public interface IProductRepository
{
    Task<PagedResult<Product>> GetAllAsync(
        ProductListFilter? filter,
        ProductSort sort,
        PageRequest page,
        CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PagedResult<Product>> GetByCategoryAsync(
        int categoryId,
        ProductListFilter? filter,
        ProductSort sort,
        PageRequest page,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Published products in a category identified by category slug. Optional <paramref name="searchTerm"/> filters like <see cref="SearchAsync"/>.
    /// </summary>
    Task<PagedResult<Product>> GetByCategorySlugAsync(
        string categorySlug,
        string? searchTerm,
        ProductListFilter? filter,
        ProductSort sort,
        PageRequest page,
        CancellationToken cancellationToken = default);

    /// <summary>Published products with optional full-text <paramref name="searchTerm"/> and filters (same as <see cref="GetAllAsync"/>).</summary>
    Task<PagedResult<Product>> SearchAsync(
        string? searchTerm,
        ProductListFilter? filter,
        ProductSort sort,
        PageRequest page,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetFeaturedAsync(
        int take = 8,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes a product: marks deleted, clears publish, and rewrites slug/SKU so unique constraints allow reuse.
    /// </summary>
    Task<ProductSoftDeleteResult> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
