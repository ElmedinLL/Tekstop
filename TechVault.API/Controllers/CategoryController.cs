using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechVault.API.Caching;
using TechVault.API.Categories;
using TechVault.API.Models;
using TechVault.API.Repositories;
using TechVault.API.Services;

namespace TechVault.API.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/categories")]
public sealed class CategoryController(
    IRepository<Category> categoryRepository,
    ICatalogListCache catalogListCache,
    IWebHostEnvironment webHostEnvironment) : ControllerBase
{
    private const int MaxCategorySlugLength = 180;

    private static readonly HashSet<string> AllowedCategoryImageExtensions =
    [
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".svg"
    ];

    /// <summary>Lists all categories with product counts (non-deleted products), ordered by display order then name.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryListItemDto>>> GetCategories(CancellationToken cancellationToken)
    {
        var cacheKey = catalogListCache.CreateCategoryListKey();
        if (catalogListCache.TryGetCategoryList(cacheKey, out var cached) && cached is not null)
        {
            return Ok(cached);
        }

        var items = await categoryRepository.QueryAsNoTracking()
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                ParentCategoryId = c.ParentCategoryId,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                ProductCount = c.Products.Count(),
            })
            .ToListAsync(cancellationToken);

        catalogListCache.SetCategoryList(cacheKey, items);
        return Ok(items);
    }

    /// <summary>Returns a category by slug with product count.</summary>
    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(CategoryDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDetailDto>> GetCategoryBySlug(string slug, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return NotFound();
        }

        var trimmed = slug.Trim();
        var dto = await categoryRepository.QueryAsNoTracking()
            .Where(c => c.Slug == trimmed)
            .Select(c => new CategoryDetailDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                ParentCategoryId = c.ParentCategoryId,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                ProductCount = c.Products.Count(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>Uploads a category image (admin). Stored under wwwroot/images/categories.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("upload-image")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CategoryImageUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryImageUploadResponse>> UploadCategoryImage(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        file ??= Request.Form.Files.FirstOrDefault();
        if (file is null || file.Length == 0)
        {
            return BadRequest("A non-empty image file is required.");
        }

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !AllowedCategoryImageExtensions.Contains(ext.ToLowerInvariant()))
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

        var dir = Path.Combine(webRoot, "images", "categories");
        Directory.CreateDirectory(dir);

        var fileName = $"category-{Guid.NewGuid():N}{ext}";
        var physicalPath = Path.Combine(dir, fileName);

        await using (var stream = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                       bufferSize: 64 * 1024, useAsync: true))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var publicUrl = "/images/categories/" + fileName;
        return Ok(new CategoryImageUploadResponse { Url = publicUrl });
    }

    /// <summary>Creates a category (admin).</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(CategoryDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryDetailDto>> CreateCategory(
        [FromBody] CreateCategoryDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest("Name is required.");
        }

        if (dto.ParentCategoryId is { } parentId
            && !await categoryRepository.AnyAsync(c => c.Id == parentId, cancellationToken))
        {
            return BadRequest($"Parent category {parentId} does not exist.");
        }

        var baseSlug = string.IsNullOrWhiteSpace(dto.Slug)
            ? ProductSlugHelper.SlugifyName(dto.Name)
            : ProductSlugHelper.SlugifyName(dto.Slug);

        var slug = TruncateCategorySlug(await EnsureUniqueCategorySlugAsync(baseSlug, excludeId: null, cancellationToken));

        var entity = new Category
        {
            Name = dto.Name.Trim(),
            Slug = slug,
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim(),
            ParentCategoryId = dto.ParentCategoryId,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive,
        };

        await categoryRepository.AddAsync(entity, cancellationToken);
        try
        {
            await categoryRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Conflict("Could not save the category (slug may be in use).");
        }

        catalogListCache.InvalidateCatalogLists();

        var detail = await LoadCategoryDetailAsync(entity.Id, cancellationToken);
        return CreatedAtAction(
            nameof(GetCategoryBySlug),
            new { version = HttpContext.GetRequestedApiVersion()!.ToString(), slug = entity.Slug },
            detail);
    }

    /// <summary>Updates a category by slug (admin).</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{slug}")]
    [ProducesResponseType(typeof(CategoryDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDetailDto>> UpdateCategory(
        string slug,
        [FromBody] UpdateCategoryDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest("Name is required.");
        }

        var trimmed = slug.Trim();
        var entity = await categoryRepository.Query().FirstOrDefaultAsync(c => c.Slug == trimmed, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        if (dto.ParentCategoryId is { } pid)
        {
            if (pid == entity.Id)
            {
                return BadRequest("A category cannot be its own parent.");
            }

            if (!await categoryRepository.AnyAsync(c => c.Id == pid, cancellationToken))
            {
                return BadRequest($"Parent category {pid} does not exist.");
            }
        }

        entity.Name = dto.Name.Trim();
        entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        entity.ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim();
        entity.ParentCategoryId = dto.ParentCategoryId;
        entity.DisplayOrder = dto.DisplayOrder;
        entity.IsActive = dto.IsActive;

        var baseSlug = string.IsNullOrWhiteSpace(dto.Slug)
            ? ProductSlugHelper.SlugifyName(entity.Name)
            : ProductSlugHelper.SlugifyName(dto.Slug);
        entity.Slug = TruncateCategorySlug(
            await EnsureUniqueCategorySlugAsync(baseSlug, excludeId: entity.Id, cancellationToken));

        categoryRepository.Update(entity);
        try
        {
            await categoryRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Conflict("Could not save the category (slug may be in use).");
        }

        catalogListCache.InvalidateCatalogLists();

        var detail = await LoadCategoryDetailAsync(entity.Id, cancellationToken);
        return Ok(detail);
    }

    /// <summary>Deletes a category by slug (admin). Fails if products or child categories reference it.</summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{slug}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCategory(string slug, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return NotFound();
        }

        var trimmed = slug.Trim();
        var entity = await categoryRepository.Query()
            .Include(c => c.Products)
            .Include(c => c.ChildCategories)
            .FirstOrDefaultAsync(c => c.Slug == trimmed, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Products.Count > 0)
        {
            return Conflict("Cannot delete a category that still has products.");
        }

        if (entity.ChildCategories.Count > 0)
        {
            return Conflict("Cannot delete a category that has child categories.");
        }

        categoryRepository.Remove(entity);
        try
        {
            await categoryRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Conflict("Cannot delete this category (it may be referenced elsewhere).");
        }

        catalogListCache.InvalidateCatalogLists();

        return NoContent();
    }

    private async Task<CategoryDetailDto> LoadCategoryDetailAsync(int id, CancellationToken cancellationToken) =>
        await categoryRepository.QueryAsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoryDetailDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                ParentCategoryId = c.ParentCategoryId,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                ProductCount = c.Products.Count(),
            })
            .FirstAsync(cancellationToken);

    private async Task<string> EnsureUniqueCategorySlugAsync(string baseSlug, int? excludeId, CancellationToken cancellationToken)
    {
        var candidate = TruncateCategorySlug(baseSlug);
        var attempt = 0;

        while (await SlugTakenAsync(candidate, excludeId, cancellationToken))
        {
            attempt++;
            var suffix = $"-{attempt + 1}";
            var prefix = ProductSlugHelper.TruncateForSuffix(candidate, suffix);
            candidate = TruncateCategorySlug(prefix + suffix);
        }

        return candidate;
    }

    private Task<bool> SlugTakenAsync(string slug, int? excludeId, CancellationToken cancellationToken)
    {
        var query = categoryRepository.QueryAsNoTracking().Where(c => c.Slug == slug);
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    private static string TruncateCategorySlug(string slug)
    {
        if (slug.Length <= MaxCategorySlugLength)
        {
            return slug;
        }

        return slug[..MaxCategorySlugLength].TrimEnd('-');
    }
}
