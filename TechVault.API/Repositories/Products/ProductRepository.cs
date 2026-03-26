using Microsoft.EntityFrameworkCore;
using TechVault.API.Data;
using TechVault.API.Models;
using TechVault.API.Repositories;
using TechVault.API.Services;

namespace TechVault.API.Repositories.Products;

public sealed class ProductRepository(ApplicationDbContext context) : IProductRepository
{
    private const int MaxSkuLength = 64;
    public async Task<PagedResult<Product>> GetAllAsync(
        ProductListFilter? filter,
        ProductSort sort,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        var query = CoreQuery();
        query = ApplyFilter(query, filter ?? new ProductListFilter());
        return await ToPagedAsync(query, sort, page, cancellationToken);
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        CoreQuery()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<PagedResult<Product>> GetByCategoryAsync(
        int categoryId,
        ProductListFilter? filter,
        ProductSort sort,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        var baseFilter = filter ?? new ProductListFilter();
        var merged = baseFilter with { CategoryId = categoryId };
        var query = CoreQuery();
        query = ApplyFilter(query, merged);
        return await ToPagedAsync(query, sort, page, cancellationToken);
    }

    public async Task<PagedResult<Product>> GetByCategorySlugAsync(
        string categorySlug,
        string? searchTerm,
        ProductListFilter? filter,
        ProductSort sort,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        var slug = categorySlug.Trim();
        if (string.IsNullOrEmpty(slug))
        {
            return EmptyPage(page);
        }

        var query = CoreQuery().Where(p => p.Category.Slug == slug);
        query = ApplyFilter(query, filter ?? new ProductListFilter());
        query = ApplySearchTerm(query, searchTerm);
        return await ToPagedAsync(query, sort, page, cancellationToken);
    }

    public async Task<PagedResult<Product>> SearchAsync(
        string searchTerm,
        ProductListFilter? filter,
        ProductSort sort,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        var term = searchTerm.Trim();
        if (string.IsNullOrEmpty(term))
        {
            return EmptyPage(page);
        }

        var query = CoreQuery();
        query = ApplyFilter(query, filter ?? new ProductListFilter());
        query = ApplySearchTerm(query, searchTerm);
        return await ToPagedAsync(query, sort, page, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetFeaturedAsync(
        int take = 8,
        CancellationToken cancellationToken = default)
    {
        var limit = take < 1 ? 8 : (take > 100 ? 100 : take);
        return await CoreQuery()
            .Where(p => p.IsPublished && p.StockQuantity > 0)
            .OrderByDescending(p => p.CreatedAtUtc)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductSoftDeleteResult> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await context.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
        {
            return ProductSoftDeleteResult.NotFound;
        }

        if (product.IsDeleted)
        {
            return ProductSoftDeleteResult.AlreadyDeleted;
        }

        var slugSuffix = $"-del-{product.Id}";
        product.Slug = ProductSlugHelper.TruncateForSuffix(product.Slug, slugSuffix) + slugSuffix;

        var skuSuffix = $"-DEL-{product.Id}";
        product.Sku = AppendSkuSuffix(product.Sku, skuSuffix);

        product.IsDeleted = true;
        product.DeletedAtUtc = DateTime.UtcNow;
        product.IsPublished = false;
        product.UpdatedAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return ProductSoftDeleteResult.Deleted;
    }

    private static string AppendSkuSuffix(string sku, string suffix)
    {
        if (sku.Length + suffix.Length <= MaxSkuLength)
        {
            return sku + suffix;
        }

        var max = MaxSkuLength - suffix.Length;
        return max <= 0 ? suffix[..MaxSkuLength] : sku[..max] + suffix;
    }

    /// <summary>
    /// Base query: no-tracking, split queries (avoids row explosion with collection includes),
    /// eager loads <see cref="Product.Category"/> and ordered <see cref="Product.Images"/>.
    /// </summary>
    private IQueryable<Product> CoreQuery() =>
        context.Products
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.SortOrder).ThenBy(i => i.Id));

    private static IQueryable<Product> ApplyFilter(IQueryable<Product> query, ProductListFilter f)
    {
        if (f.CategoryId is { } categoryId)
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        if (f.MinPrice is { } min)
        {
            query = query.Where(p => p.Price >= min);
        }

        if (f.MaxPrice is { } max)
        {
            query = query.Where(p => p.Price <= max);
        }

        if (!string.IsNullOrWhiteSpace(f.Brand))
        {
            var brand = f.Brand.Trim();
            query = query.Where(p => p.Brand != null && p.Brand == brand);
        }

        if (f.InStockOnly == true)
        {
            query = query.Where(p => p.StockQuantity > 0);
        }

        if (f.PublishedOnly)
        {
            query = query.Where(p => p.IsPublished);
        }

        return query;
    }

    private static IQueryable<Product> ApplySearchTerm(IQueryable<Product> query, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return query;
        }

        var term = searchTerm.Trim();
        return query.Where(
            p => p.Name.Contains(term)
                || p.Sku.Contains(term)
                || (p.ShortDescription != null && p.ShortDescription.Contains(term))
                || (p.Brand != null && p.Brand.Contains(term)));
    }

    private static IQueryable<Product> ApplySort(IQueryable<Product> query, ProductSort sort) =>
        sort switch
        {
            ProductSort.OldestFirst => query.OrderBy(p => p.CreatedAtUtc),
            ProductSort.NameAscending => query.OrderBy(p => p.Name),
            ProductSort.NameDescending => query.OrderByDescending(p => p.Name),
            ProductSort.PriceAscending => query.OrderBy(p => p.Price),
            ProductSort.PriceDescending => query.OrderByDescending(p => p.Price),
            ProductSort.StockDescending => query.OrderByDescending(p => p.StockQuantity),
            ProductSort.NewestFirst or _ => query.OrderByDescending(p => p.CreatedAtUtc),
        };

    private async Task<PagedResult<Product>> ToPagedAsync(
        IQueryable<Product> query,
        ProductSort sort,
        PageRequest page,
        CancellationToken cancellationToken)
    {
        var total = await query.CountAsync(cancellationToken);
        var sorted = ApplySort(query, sort);
        var items = await sorted
            .Skip(page.Skip)
            .Take(page.Take)
            .ToListAsync(cancellationToken);

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = total,
            PageNumber = page.NormalizedPage,
            PageSize = page.NormalizedPageSize,
        };
    }

    private static PagedResult<Product> EmptyPage(PageRequest page) =>
        new()
        {
            Items = Array.Empty<Product>(),
            TotalCount = 0,
            PageNumber = page.NormalizedPage,
            PageSize = page.NormalizedPageSize,
        };
}
