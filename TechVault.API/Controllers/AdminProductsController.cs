using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechVault.API.Products;
using TechVault.API.Repositories.Products;
using TechVault.API.Services;

namespace TechVault.API.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "Admin")]
public sealed class AdminProductsController(
    ILowStockInventoryService lowStockInventoryService,
    IProductRepository productCatalog,
    IMapper mapper) : ControllerBase
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    /// <summary>
    /// Paged product catalog for admins (published and draft). Query: search, sort, page, pageSize.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedProductsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedProductsResponse>> List(
        [FromQuery] AdminProductListQueryParameters query,
        CancellationToken cancellationToken)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? DefaultPageSize : Math.Min(query.PageSize, MaxPageSize);
        var sortKey = query.Sort?.Trim().ToLowerInvariant() ?? "newest";
        var sort = ParseAdminSort(sortKey);

        var filter = new ProductListFilter(
            CategoryId: null,
            CategorySlug: null,
            MinPrice: null,
            MaxPrice: null,
            Brand: null,
            InStockOnly: null,
            PublishedOnly: false,
            SearchTerm: string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
            SpecFilters: null);

        var pageRequest = new PageRequest(page, pageSize);
        var result = await productCatalog.GetAllAsync(filter, sort, pageRequest, cancellationToken);

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

    /// <summary>Products with stock quantity below the configured low-stock threshold (default 10).</summary>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(IReadOnlyList<LowStockProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<LowStockProductDto>>> GetLowStock(CancellationToken cancellationToken)
    {
        var items = await lowStockInventoryService.GetLowStockProductsAsync(cancellationToken);
        return Ok(items);
    }

    private static ProductSort ParseAdminSort(string sortKey) =>
        sortKey switch
        {
            "price_asc" => ProductSort.PriceAscending,
            "price_desc" => ProductSort.PriceDescending,
            "name_asc" => ProductSort.NameAscending,
            "name_desc" => ProductSort.NameDescending,
            "name" => ProductSort.NameAscending,
            "stock_asc" => ProductSort.StockAscending,
            "stock_desc" => ProductSort.StockDescending,
            "category_asc" => ProductSort.CategoryAscending,
            "category_desc" => ProductSort.CategoryDescending,
            "oldest" => ProductSort.OldestFirst,
            "newest" => ProductSort.NewestFirst,
            _ => ProductSort.NewestFirst
        };
}
