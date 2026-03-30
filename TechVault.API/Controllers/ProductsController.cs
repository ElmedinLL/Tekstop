using System.Globalization;
using AutoMapper;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechVault.API.Caching;
using TechVault.API.Models;
using TechVault.API.Products;
using TechVault.API.Repositories;
using TechVault.API.Repositories.Products;
using TechVault.API.Services;

namespace TechVault.API.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/products")]
public sealed class ProductsController(
    IRepository<Product> productRepository,
    IRepository<Category> categoryRepository,
    IRepository<ProductImage> productImageRepository,
    IProductRepository productCatalog,
    IAdminProductService adminProductService,
    ICatalogListCache catalogListCache,
    IMapper mapper,
    IWebHostEnvironment webHostEnvironment) : ControllerBase
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;
    private const int MaxImagesPerProduct = 5;

    private static readonly HashSet<string> AllowedImageExtensions =
    [
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".svg"
    ];

    /// <summary>
    /// Returns a single published product by id. When missing or not published, responds with 200 and JSON null
    /// so browsers do not log a failed (404) request for SPA lookups.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductDto?>> GetProductById(int id, CancellationToken cancellationToken)
    {
        var product = await productCatalog.GetByIdAsync(id, cancellationToken);
        if (product is null || !product.IsPublished)
        {
            return Ok((ProductDto?)null);
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
    /// Published products in a category (by category slug). Query: search (full-text on name/description),
    /// specs (repeat <c>Key:Value</c>, e.g. RAM:16GB), minPrice, maxPrice,
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
            CategorySlug: null,
            MinPrice: query.MinPrice,
            MaxPrice: query.MaxPrice,
            Brand: null,
            InStockOnly: null,
            PublishedOnly: true,
            SearchTerm: string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
            SpecFilters: ProductSpecQueryParser.Parse(query.Specs));

        var pageRequest = new PageRequest(page, pageSize);
        var result = await productCatalog.GetByCategorySlugAsync(
            trimmed,
            searchTerm: null,
            filter: filter,
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
    /// Lists published products with optional filters. Query: category (id or slug), search (full-text on name/description),
    /// specs (repeat <c>Key:Value</c>), minPrice, maxPrice,
    /// sort (price_asc|price_desc|name_asc|name_desc|newest|oldest|stock_desc), page, pageSize.
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

        var cacheKey = catalogListCache.CreateProductListKey(query);
        if (catalogListCache.TryGetProductList(cacheKey, out var cachedList) && cachedList is not null)
        {
            return Ok(cachedList);
        }

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? DefaultPageSize : Math.Min(query.PageSize, MaxPageSize);
        var sortKey = query.Sort?.Trim().ToLowerInvariant() ?? "newest";
        var sort = ParseSort(sortKey);

        int? categoryId = null;
        string? categorySlug = null;
        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            var c = query.Category.Trim();
            if (int.TryParse(c, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
            {
                categoryId = id;
            }
            else
            {
                categorySlug = c;
            }
        }

        var filter = new ProductListFilter(
            CategoryId: categoryId,
            CategorySlug: categorySlug,
            MinPrice: query.MinPrice,
            MaxPrice: query.MaxPrice,
            Brand: null,
            InStockOnly: null,
            PublishedOnly: true,
            SearchTerm: string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
            SpecFilters: ProductSpecQueryParser.Parse(query.Specs));

        var pageRequest = new PageRequest(page, pageSize);
        var result = await productCatalog.GetAllAsync(filter, sort, pageRequest, cancellationToken);

        var items = mapper.Map<IReadOnlyList<ProductListItemDto>>(result.Items);
        var totalPages = result.TotalCount == 0 ? 0 : (int)Math.Ceiling(result.TotalCount / (double)result.PageSize);

        var paged = new PagedProductsResponse
        {
            Items = items,
            TotalCount = result.TotalCount,
            Page = result.PageNumber,
            PageSize = result.PageSize,
            TotalPages = totalPages
        };
        catalogListCache.SetProductList(cacheKey, paged);
        return Ok(paged);
    }

    /// <summary>Creates a product (admin).</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> CreateProduct(
        [FromBody] CreateProductDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = await adminProductService.CreateAsync(dto, cancellationToken);
            catalogListCache.InvalidateCatalogLists();
            var detail = await productCatalog.GetByIdAsync(product.Id, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, mapper.Map<ProductDto>(detail!));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (DbUpdateException)
        {
            return Conflict("Could not save the product.");
        }
    }

    /// <summary>Updates a product (admin).</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> UpdateProduct(
        int id,
        [FromBody] UpdateProductDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = await adminProductService.UpdateAsync(id, dto, cancellationToken);
            if (product is null)
            {
                return NotFound();
            }

            catalogListCache.InvalidateCatalogLists();
            var detail = await productCatalog.GetByIdAsync(product.Id, cancellationToken);
            return Ok(mapper.Map<ProductDto>(detail!));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (DbUpdateException)
        {
            return Conflict("Could not save the product.");
        }
    }

    /// <summary>Uploads a product image (admin). Stored under wwwroot/images; up to 5 images per product.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/images")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ProductImageUploadResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductImageUploadResponse>> UploadProductImage(
        int id,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("A non-empty image file is required.");
        }

        if (!await productRepository.AnyAsync(p => p.Id == id, cancellationToken))
        {
            return NotFound();
        }

        var imageCount = await productImageRepository.CountAsync(i => i.ProductId == id, cancellationToken);
        if (imageCount >= MaxImagesPerProduct)
        {
            return BadRequest($"A maximum of {MaxImagesPerProduct} images per product is allowed.");
        }

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !AllowedImageExtensions.Contains(ext.ToLowerInvariant()))
        {
            return BadRequest("Allowed image types: JPEG, PNG, GIF, WebP, BMP, SVG.");
        }

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("The uploaded file must be an image.");
        }

        var webRoot = webHostEnvironment.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
        {
            webRoot = Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");
        }

        var imagesDir = Path.Combine(webRoot, "images");
        Directory.CreateDirectory(imagesDir);

        var fileName = $"product-{id.ToString(CultureInfo.InvariantCulture)}-{Guid.NewGuid():N}{ext}";
        var physicalPath = Path.Combine(imagesDir, fileName);

        await using (var stream = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                       bufferSize: 64 * 1024, useAsync: true))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var publicUrl = "/images/" + fileName;

        var maxSort = await productImageRepository.QueryAsNoTracking()
            .Where(i => i.ProductId == id)
            .Select(i => (int?)i.SortOrder)
            .MaxAsync(cancellationToken);
        var sortOrder = (maxSort ?? -1) + 1;

        var entity = new ProductImage
        {
            ProductId = id,
            Url = publicUrl,
            SortOrder = sortOrder
        };

        await productImageRepository.AddAsync(entity, cancellationToken);
        await productImageRepository.SaveChangesAsync(cancellationToken);

        catalogListCache.InvalidateCatalogLists();

        return StatusCode(StatusCodes.Status201Created,
            new ProductImageUploadResponse { Id = entity.Id, Url = entity.Url, SortOrder = entity.SortOrder });
    }

    /// <summary>Soft-deletes a product (admin): hides from storefront and frees slug/SKU for reuse.</summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        var result = await productCatalog.SoftDeleteAsync(id, cancellationToken);
        if (result == ProductSoftDeleteResult.Deleted)
        {
            catalogListCache.InvalidateCatalogLists();
        }

        return result switch
        {
            ProductSoftDeleteResult.NotFound => NotFound(),
            ProductSoftDeleteResult.AlreadyDeleted => NoContent(),
            ProductSoftDeleteResult.Deleted => NoContent(),
            _ => throw new InvalidOperationException($"Unexpected delete result: {result}."),
        };
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
            "stock_asc" => ProductSort.StockAscending,
            "stock_desc" => ProductSort.StockDescending,
            "category_asc" => ProductSort.CategoryAscending,
            "category_desc" => ProductSort.CategoryDescending,
            "newest" => ProductSort.NewestFirst,
            _ => ProductSort.NewestFirst
        };

}
