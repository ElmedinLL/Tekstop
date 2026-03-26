using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechVault.API.Models;
using TechVault.API.Repositories;

namespace TechVault.API.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(IRepository<Product> productRepository) : ControllerBase
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

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
