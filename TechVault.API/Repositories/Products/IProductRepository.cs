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

    Task<PagedResult<Product>> SearchAsync(
        string searchTerm,
        ProductListFilter? filter,
        ProductSort sort,
        PageRequest page,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetFeaturedAsync(
        int take = 8,
        CancellationToken cancellationToken = default);
}
