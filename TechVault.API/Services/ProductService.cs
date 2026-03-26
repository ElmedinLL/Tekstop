using Microsoft.EntityFrameworkCore;
using TechVault.API.Models;
using TechVault.API.Repositories;

namespace TechVault.API.Services;

public sealed class ProductService(
    IRepository<Product> productRepository,
    IRepository<Category> categoryRepository) : IProductService
{
    public async Task<Product> CreateAsync(CreateProductInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        ValidatePrice(input.Price);

        if (!await categoryRepository.AnyAsync(c => c.Id == input.CategoryId, cancellationToken))
        {
            throw new InvalidOperationException($"Category {input.CategoryId} does not exist.");
        }

        var baseSlug = string.IsNullOrWhiteSpace(input.Slug)
            ? ProductSlugHelper.SlugifyName(input.Name)
            : ProductSlugHelper.SlugifyName(input.Slug);

        var uniqueSlug = await EnsureUniqueSlugAsync(baseSlug, excludeProductId: null, cancellationToken);

        var now = DateTime.UtcNow;
        var product = new Product
        {
            CategoryId = input.CategoryId,
            Name = input.Name.Trim(),
            Slug = uniqueSlug,
            Sku = input.Sku.Trim(),
            ShortDescription = input.ShortDescription?.Trim(),
            Description = input.Description?.Trim(),
            Price = input.Price,
            CompareAtPrice = input.CompareAtPrice,
            StockQuantity = input.StockQuantity,
            ImageUrl = string.IsNullOrWhiteSpace(input.ImageUrl) ? null : input.ImageUrl.Trim(),
            Brand = string.IsNullOrWhiteSpace(input.Brand) ? null : input.Brand.Trim(),
            IsPublished = input.IsPublished,
            CreatedAtUtc = now,
            UpdatedAtUtc = null
        };

        await productRepository.AddAsync(product, cancellationToken);
        await productRepository.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<Product> UpdateAsync(int productId, UpdateProductInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        ValidatePrice(input.Price);

        if (!await categoryRepository.AnyAsync(c => c.Id == input.CategoryId, cancellationToken))
        {
            throw new InvalidOperationException($"Category {input.CategoryId} does not exist.");
        }

        var product = await productRepository.FindAsync([productId], cancellationToken);
        if (product is null)
        {
            throw new InvalidOperationException($"Product {productId} was not found.");
        }

        var baseSlug = string.IsNullOrWhiteSpace(input.Slug)
            ? ProductSlugHelper.SlugifyName(input.Name)
            : ProductSlugHelper.SlugifyName(input.Slug);

        var uniqueSlug = await EnsureUniqueSlugAsync(baseSlug, excludeProductId: productId, cancellationToken);

        product.CategoryId = input.CategoryId;
        product.Name = input.Name.Trim();
        product.Slug = uniqueSlug;
        product.Sku = input.Sku.Trim();
        product.ShortDescription = input.ShortDescription?.Trim();
        product.Description = input.Description?.Trim();
        product.Price = input.Price;
        product.CompareAtPrice = input.CompareAtPrice;
        product.StockQuantity = input.StockQuantity;
        product.ImageUrl = string.IsNullOrWhiteSpace(input.ImageUrl) ? null : input.ImageUrl.Trim();
        product.Brand = string.IsNullOrWhiteSpace(input.Brand) ? null : input.Brand.Trim();
        product.IsPublished = input.IsPublished;
        product.UpdatedAtUtc = DateTime.UtcNow;

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
}
