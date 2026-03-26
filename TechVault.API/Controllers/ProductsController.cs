using System.Globalization;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechVault.API.Models;
using TechVault.API.Products;
using TechVault.API.Repositories;
using TechVault.API.Repositories.Products;

namespace TechVault.API.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(
    IRepository<Product> productRepository,
    IRepository<Category> categoryRepository,
    IProductRepository productCatalog,
    IMapper mapper) : ControllerBase
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    /// <summary>Returns a single published product by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProductById(int id, CancellationToken cancellationToken)
    {
        var product = await productCatalog.GetByIdAsync(id, cancellationToken);
        if (product is null || !product.IsPublished)
        {
            return NotFound();
        }

        return Ok(mapper.Map<ProductDto>(product));
    }

    /// <summary>Featured products for the homepage (published, in stock, newest first).</summary>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductListItemDto>>> GetFeaturedProducts(
        [FromQuery] int take = 8,
        CancellationToken cancellationToken = default)
    {
        var limit = take < 1 ? 8 : Math.Min(take, MaxPageSize);
        var products = await productCatalog.GetFeaturedAsync(limit, cancellationToken);
        return Ok(mapper.Map<IReadOnlyList<ProductListItemDto>>(products));
    }

    /// <summary>
    /// Published products in a category (by category slug). Query: search, minPrice, maxPrice,
    /// sort (price_asc|price_desc|name_asc|name_desc|newest|oldest|stock_desc), page, pageSize.
    /// </summary>
    [HttpGet("category/{slug}")]
    [ProducesResponseType(typeof(PagedProductsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedProductsResponse>> GetProductsByCategory(
        string slug,
        [FromQuery] ProductCategoryPageQueryParameters query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return BadRequest("Category slug is required.");
        }

        var trimmed = slug.Trim();
        if (!await categoryRepository.AnyAsync(c => c.Slug == trimmed, cancellationToken))
        {
            return NotFound();
        }

        if (query.MinPrice.HasValue && query.MaxPrice.HasValue && query.MinPrice > query.MaxPrice)
        {
            return BadRequest("minPrice cannot be greater than maxPrice.");
        }

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? DefaultPageSize : Math.Min(query.PageSize, MaxPageSize);
        var sortKey = query.Sort?.Trim().ToLowerInvariant() ?? "newest";
        var sort = ParseSort(sortKey);

        var filter = new ProductListFilter(
            CategoryId: null,
            MinPrice: query.MinPrice,
            MaxPrice: query.MaxPrice,
            Brand: null,
            InStockOnly: null,
            PublishedOnly: true);

        var pageRequest = new PageRequest(page, pageSize);
        var result = await productCatalog.GetByCategorySlugAsync(
            trimmed,
            query.Search,
            filter,
            sort,
            pageRequest,
            cancellationToken);

        var items = mapper.Map<IReadOnlyList<ProductListItemDto>>(result.Items);
        var totalPages = result.TotalCount == 0 ? 0 : (int)Math.Ceiling(result.TotalCount / (double)result.PageSize);

        return Ok(new PagedProductsResponse
        {
            Items = items,
            TotalCount = result.TotalCount,
            Page = result.PageNumber,
            PageSize = result.PageSize,
            TotalPages = totalPages
        });
    }

    /// <summary>
    /// Lists published products with optional filters. Query: category (id or slug), search, minPrice, maxPrice,
    /// sort (price_asc|price_desc|name_asc|name_desc|newest|oldest), page, pageSize.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedProductsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedProductsResponse>> GetProducts(
        [FromQuery] ProductListQueryParameters query,
        CancellationToken cancellationToken)
    {
        if (query.MinPrice.HasValue && query.MaxPrice.HasValue && query.MinPrice > query.MaxPrice)
        {
            return BadRequest("minPrice cannot be greater than maxPrice.");
        }

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? DefaultPageSize : Math.Min(query.PageSize, MaxPageSize);

        var baseQuery = productRepository.QueryAsNoTracking().Where(p => p.IsPublished);

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            var c = query.Category.Trim();
            if (int.TryParse(c, NumberStyles.Integer, CultureInfo.InvariantCulture, out var categoryId))
            {
                baseQuery = baseQuery.Where(p => p.CategoryId == categoryId);
            }
            else
            {
                baseQuery = baseQuery.Where(p => p.Category.Slug == c);
            }
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            baseQuery = baseQuery.Where(p =>
                p.Name.Contains(term)
                || (p.ShortDescription != null && p.ShortDescription.Contains(term))
                || p.Sku.Contains(term)
                || (p.Brand != null && p.Brand.Contains(term)));
        }

        if (query.MinPrice.HasValue)
        {
            baseQuery = baseQuery.Where(p => p.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            baseQuery = baseQuery.Where(p => p.Price <= query.MaxPrice.Value);
        }

        var sortKey = query.Sort?.Trim().ToLowerInvariant() ?? "newest";
        var orderedQuery = ApplySort(baseQuery, sortKey);

        var totalCount = await orderedQuery.CountAsync(cancellationToken);

        var items = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListItemDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Price = p.Price,
                CompareAtPrice = p.CompareAtPrice,
                ImageUrl = p.ImageUrl,
                Brand = p.Brand,
                CategoryName = p.Category.Name,
                CategorySlug = p.Category.Slug,
                StockQuantity = p.StockQuantity,
                IsPublished = p.IsPublished
            })
            .ToListAsync(cancellationToken);

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        return Ok(new PagedProductsResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        });
    }

    private static ProductSort ParseSort(string sortKey) =>
        sortKey switch
        {
            "price_asc" => ProductSort.PriceAscending,
            "price_desc" => ProductSort.PriceDescending,
            "name_asc" => ProductSort.NameAscending,
            "name_desc" => ProductSort.NameDescending,
            "name" => ProductSort.NameAscending,
            "oldest" => ProductSort.OldestFirst,
            "stock_desc" => ProductSort.StockDescending,
            "newest" => ProductSort.NewestFirst,
            _ => ProductSort.NewestFirst
        };

    private static IOrderedQueryable<Product> ApplySort(IQueryable<Product> query, string sortKey)
    {
        return sortKey switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "name_asc" => query.OrderBy(p => p.Name),
            "name_desc" => query.OrderByDescending(p => p.Name),
            "name" => query.OrderBy(p => p.Name),
            "oldest" => query.OrderBy(p => p.CreatedAtUtc),
            "newest" => query.OrderByDescending(p => p.CreatedAtUtc),
            _ => query.OrderByDescending(p => p.CreatedAtUtc)
        };
    }
}
