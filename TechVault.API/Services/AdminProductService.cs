using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TechVault.API.Models;
using TechVault.API.Products;
using TechVault.API.Repositories;

namespace TechVault.API.Services;

public sealed class AdminProductService(
    IMapper mapper,
    IRepository<Product> productRepository,
    IRepository<Category> categoryRepository,
    IRepository<ProductImage> productImageRepository) : IAdminProductService
{
    public async Task<Product> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        ValidatePrice(dto.Price);

        if (!await categoryRepository.AnyAsync(c => c.Id == dto.CategoryId, cancellationToken))
        {
            throw new InvalidOperationException($"Category {dto.CategoryId} does not exist.");
        }

        var sku = dto.Sku.Trim();
        if (await SkuTakenAsync(sku, excludeProductId: null, cancellationToken))
        {
            throw new InvalidOperationException($"SKU \"{sku}\" is already in use.");
        }

        var product = mapper.Map<Product>(dto);

        var baseSlug = string.IsNullOrWhiteSpace(dto.Slug)
            ? ProductSlugHelper.SlugifyName(dto.Name)
            : ProductSlugHelper.SlugifyName(dto.Slug);
        product.Slug = await EnsureUniqueSlugAsync(baseSlug, excludeProductId: null, cancellationToken);

        await productRepository.AddAsync(product, cancellationToken);
        await productRepository.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<Product?> UpdateAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var product = await productRepository.FindAsync([id], cancellationToken);
        if (product is null)
        {
            return null;
        }

        if (dto.CategoryId is { } catId
            && !await categoryRepository.AnyAsync(c => c.Id == catId, cancellationToken))
        {
            throw new InvalidOperationException($"Category {catId} does not exist.");
        }

        if (dto.Price.HasValue)
        {
            ValidatePrice(dto.Price.Value);
        }

        if (dto.Sku is not null)
        {
            var sku = dto.Sku.Trim();
            if (await SkuTakenAsync(sku, id, cancellationToken))
            {
                throw new InvalidOperationException($"SKU \"{sku}\" is already in use.");
            }
        }

        if (dto.Images is not null)
        {
            var existing = await productImageRepository.Query()
                .Where(i => i.ProductId == id)
                .ToListAsync(cancellationToken);
            if (existing.Count > 0)
            {
                productImageRepository.RemoveRange(existing);
            }
        }

        mapper.Map(dto, product);

        if (dto.Slug is not null || dto.Name is not null)
        {
            var baseSlug = dto.Slug is not null
                ? ProductSlugHelper.SlugifyName(dto.Slug)
                : ProductSlugHelper.SlugifyName(dto.Name!);
            product.Slug = await EnsureUniqueSlugAsync(baseSlug, id, cancellationToken);
        }

        productRepository.Update(product);
        await productRepository.SaveChangesAsync(cancellationToken);
        return product;
    }

    private static void ValidatePrice(decimal price)
    {
        if (price <= 0)
        {
            throw new InvalidOperationException("Price must be greater than zero.");
        }
    }

    private async Task<string> EnsureUniqueSlugAsync(
        string baseSlug,
        int? excludeProductId,
        CancellationToken cancellationToken)
    {
        var candidate = baseSlug;
        var attempt = 0;

        while (await SlugTakenAsync(candidate, excludeProductId, cancellationToken))
        {
            attempt++;
            var suffix = $"-{attempt + 1}";
            var prefix = ProductSlugHelper.TruncateForSuffix(baseSlug, suffix);
            candidate = prefix + suffix;
        }

        return candidate;
    }

    private Task<bool> SlugTakenAsync(string slug, int? excludeProductId, CancellationToken cancellationToken)
    {
        var query = productRepository.QueryAsNoTracking().Where(p => p.Slug == slug);
        if (excludeProductId.HasValue)
        {
            query = query.Where(p => p.Id != excludeProductId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    private Task<bool> SkuTakenAsync(string sku, int? excludeProductId, CancellationToken cancellationToken)
    {
        var query = productRepository.QueryAsNoTracking().Where(p => p.Sku == sku);
        if (excludeProductId.HasValue)
        {
            query = query.Where(p => p.Id != excludeProductId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }
}
