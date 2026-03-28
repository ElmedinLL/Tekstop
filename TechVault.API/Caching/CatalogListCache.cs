using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using TechVault.API.Categories;
using TechVault.API.Products;

namespace TechVault.API.Caching;

public sealed class CatalogListCache(IMemoryCache memoryCache) : ICatalogListCache
{
    private const string ProductKeyPrefix = "catalog:products:";
    private const string CategoryKeyPrefix = "catalog:categories:";

    /// <summary>Matches storefront product list paging/sort normalization in ProductsController.</summary>
    private const int DefaultProductPageSize = 20;

    private const int MaxProductPageSize = 100;

    private static readonly TimeSpan ListTtl = TimeSpan.FromMinutes(5);

    private long _listingVersion;

    public string CreateProductListKey(ProductListQueryParameters query)
    {
        var canonical = BuildCanonicalProductQuery(query);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
        return $"{ProductKeyPrefix}v{Interlocked.Read(ref _listingVersion)}:{hash}";
    }

    public string CreateCategoryListKey() => $"{CategoryKeyPrefix}v{Interlocked.Read(ref _listingVersion)}";

    public bool TryGetProductList(string key, out PagedProductsResponse? value) =>
        memoryCache.TryGetValue(key, out value);

    public void SetProductList(string key, PagedProductsResponse value)
    {
        memoryCache.Set(
            key,
            value,
            new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = ListTtl });
    }

    public bool TryGetCategoryList(string key, out IReadOnlyList<CategoryListItemDto>? value) =>
        memoryCache.TryGetValue(key, out value);

    public void SetCategoryList(string key, IReadOnlyList<CategoryListItemDto> value)
    {
        memoryCache.Set(
            key,
            value,
            new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = ListTtl });
    }

    public void InvalidateCatalogLists() => Interlocked.Increment(ref _listingVersion);

    private static string BuildCanonicalProductQuery(ProductListQueryParameters q)
    {
        var category = q.Category?.Trim() ?? "";
        var search = q.Search?.Trim() ?? "";
        var min = q.MinPrice?.ToString(CultureInfo.InvariantCulture) ?? "";
        var max = q.MaxPrice?.ToString(CultureInfo.InvariantCulture) ?? "";
        var sort = string.IsNullOrWhiteSpace(q.Sort) ? "newest" : q.Sort.Trim().ToLowerInvariant();
        var specs = q.Specs is { Count: > 0 }
            ? string.Join(
                "\u001e",
                q.Specs.Select(s => s?.Trim() ?? "").Order(StringComparer.Ordinal))
            : "";
        var page = q.Page < 1 ? 1 : q.Page;
        var pageSize = q.PageSize < 1 ? DefaultProductPageSize : Math.Min(q.PageSize, MaxProductPageSize);
        return string.Join(
            '\u001f',
            category,
            search,
            min,
            max,
            specs,
            sort,
            page.ToString(CultureInfo.InvariantCulture),
            pageSize.ToString(CultureInfo.InvariantCulture));
    }
}
