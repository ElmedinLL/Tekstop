using System.Diagnostics.CodeAnalysis;
using TechVault.API.Categories;
using TechVault.API.Products;

namespace TechVault.API.Caching;

public interface ICatalogListCache
{
    string CreateProductListKey(ProductListQueryParameters query);

    string CreateCategoryListKey();

    bool TryGetProductList(string key, [NotNullWhen(true)] out PagedProductsResponse? value);

    void SetProductList(string key, PagedProductsResponse value);

    bool TryGetCategoryList(string key, [NotNullWhen(true)] out IReadOnlyList<CategoryListItemDto>? value);

    void SetCategoryList(string key, IReadOnlyList<CategoryListItemDto> value);

    /// <summary>Invalidate cached storefront product and category listings (mutual counts and filters).</summary>
    void InvalidateCatalogLists();
}
