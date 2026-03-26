using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechVault.API.Data;
using TechVault.API.Products;
using TechVault.API.Repositories;
using TechVault.API.Repositories.Products;

namespace TechVault.API.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public ProductsController(
        IProductRepository productRepository,
        ApplicationDbContext dbContext,
        IMapper mapper)
    {
        _productRepository = productRepository;
        _dbContext = dbContext;
        _mapper = mapper;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(
        [FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product is null || !product.IsPublished)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<ProductDto>(product));
    }

    [HttpGet("featured")]
    public async Task<ActionResult<IReadOnlyList<ProductListDto>>> GetFeatured(
        [FromQuery] int take = 8,
        CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetFeaturedAsync(take, cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<ProductListDto>>(products));
    }

    [HttpGet("category/{slug}")]
    public async Task<ActionResult<PagedResult<ProductListDto>>> GetByCategory(
        [FromRoute] string slug,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sort = nameof(ProductSort.NewestFirst),
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string? brand = null,
        [FromQuery] bool? inStockOnly = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return BadRequest("Category slug is required.");
        }

        var category = await _dbContext.Categories
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Slug == slug, cancellationToken);

        if (category is null)
        {
            return NotFound();
        }

        var sortEnum = ParseSort(sort);

        var filter = new ProductListFilter(
            CategoryId: null,
            MinPrice: minPrice,
            MaxPrice: maxPrice,
            Brand: brand,
            InStockOnly: inStockOnly,
            PublishedOnly: true);

        var page = new PageRequest(pageNumber, pageSize);
        var productPage = await _productRepository.GetByCategoryAsync(category.Id, filter, sortEnum, page, cancellationToken);

        var items = _mapper.Map<List<ProductListDto>>(productPage.Items);
        return Ok(new PagedResult<ProductListDto>
        {
            Items = items,
            TotalCount = productPage.TotalCount,
            PageNumber = productPage.PageNumber,
            PageSize = productPage.PageSize
        });
    }

    private static ProductSort ParseSort(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return ProductSort.NewestFirst;
        }

        return Enum.TryParse<ProductSort>(sort, ignoreCase: true, out var parsed)
            ? parsed
            : ProductSort.NewestFirst;
    }
}
