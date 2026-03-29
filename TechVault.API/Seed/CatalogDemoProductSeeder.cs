using Microsoft.EntityFrameworkCore;
using TechVault.API.Data;
using TechVault.API.Models;
using TechVault.API.Services;

namespace TechVault.API.Seed;

public sealed class CatalogDemoProductSeeder(
    ApplicationDbContext db,
    ILogger<CatalogDemoProductSeeder> logger)
{
    private static readonly string[] Adjectives =
    [
        "Pro", "Ultra", "Elite", "Swift", "Prime", "Quantum", "Nova", "Apex", "Zen", "Pulse"
    ];

    private static readonly string[] Suffixes =
    [
        "Kit", "Series", "Edition", "Plus", "Mini", "Max", "One", "Air", "Core", "Edge"
    ];

    private static readonly string[] Brands =
    [
        "TechVault", "NovaGraph", "CorePeak", "FlashForge", "PixelPro", "LinkHub", "KeyForge", "SoundArc"
    ];

    /// <summary>Primary product image per category slug — matches real catalog filenames under wwwroot/images/products.</summary>
    private static string ImageUrlForCategorySlug(string categorySlug)
    {
        var s = categorySlug.Trim().ToLowerInvariant();
        return s switch
        {
            "laptops" => "/images/products/probook-14.jpg",
            "pc-components" => "/images/products/gpu-780.jpg",
            "peripherals" => "/images/products/headset.jpg",
            "monitors" => "/images/products/mon-27.jpg",
            "networking" => "/images/products/router.jpg",
            _ => "/images/products/probook-14.jpg",
        };
    }

    public async Task SeedAsync(CatalogDemoSeedOptions options, CancellationToken cancellationToken = default)
    {
        if (!options.Enabled || options.ProductsPerCategory <= 0)
        {
            return;
        }

        var prefix = string.IsNullOrWhiteSpace(options.SkuPrefix) ? "DEMO" : options.SkuPrefix.Trim();
        var skuStartsWith = prefix + "-";

        var rnd = options.RandomSeed is { } seed
            ? new Random(seed)
            : new Random();

        var categories = await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Id)
            .ToListAsync(cancellationToken);

        if (categories.Count == 0)
        {
            logger.LogWarning("Catalog demo seed skipped: no categories in database.");
            return;
        }

        var now = DateTime.UtcNow;
        var totalAdded = 0;

        foreach (var category in categories)
        {
            var existingDemo = await db.Products
                .IgnoreQueryFilters()
                .CountAsync(p => p.CategoryId == category.Id && p.Sku.StartsWith(skuStartsWith), cancellationToken);

            var toAdd = Math.Max(0, options.ProductsPerCategory - existingDemo);
            if (toAdd == 0)
            {
                continue;
            }

            for (var i = 0; i < toAdd; i++)
            {
                var name = BuildProductName(rnd, category.Name);
                var baseSlug = ProductSlugHelper.SlugifyName(name);
                var slug = await EnsureUniqueSlugAsync(baseSlug, cancellationToken);
                var sku = $"{prefix}-{category.Id}-{Guid.NewGuid():N}";
                if (sku.Length > 64)
                {
                    sku = sku[..64];
                }

                sku = await EnsureUniqueSkuAsync(sku, cancellationToken);

                var price = RandomPrice(rnd);
                decimal? compare = rnd.Next(3) == 0 ? null : Math.Round(price * (1 + rnd.Next(5, 15) / 100m), 2);

                var product = new Product
                {
                    CategoryId = category.Id,
                    Name = name,
                    Slug = slug,
                    ShortDescription = $"Demo item for {category.Name}.",
                    Description = $"<p>Auto-generated demo product in category <strong>{category.Name}</strong>.</p>",
                    Sku = sku,
                    Price = price,
                    CompareAtPrice = compare,
                    StockQuantity = rnd.Next(5, 120),
                    Brand = Brands[rnd.Next(Brands.Length)],
                    IsPublished = true,
                    IsDeleted = false,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now,
                    ImageUrl = ImageUrlForCategorySlug(category.Slug),
                    ReviewCount = 0,
                    AverageRating = null
                };

                await db.Products.AddAsync(product, cancellationToken);
                totalAdded++;
            }
        }

        if (totalAdded > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Catalog demo seed added {Count} product(s) across categories (prefix {Prefix}).", totalAdded, skuStartsWith);
        }
    }

    private static string BuildProductName(Random rnd, string categoryName)
    {
        var adj = Adjectives[rnd.Next(Adjectives.Length)];
        var sfx = Suffixes[rnd.Next(Suffixes.Length)];
        var n = rnd.Next(100, 999);
        return $"{adj} {categoryName.Trim()} {sfx} {n}";
    }

    private static decimal RandomPrice(Random rnd)
    {
        var major = rnd.Next(9, 2499);
        var cents = rnd.Next(0, 100);
        return major + cents / 100m;
    }

    private async Task<string> EnsureUniqueSlugAsync(string baseSlug, CancellationToken cancellationToken)
    {
        var candidate = baseSlug;
        var attempt = 0;

        while (await db.Products.IgnoreQueryFilters().AnyAsync(p => p.Slug == candidate, cancellationToken))
        {
            attempt++;
            var suffix = $"-{attempt + 1}";
            var prefix = ProductSlugHelper.TruncateForSuffix(baseSlug, suffix);
            candidate = prefix + suffix;
        }

        return candidate;
    }

    private async Task<string> EnsureUniqueSkuAsync(string sku, CancellationToken cancellationToken)
    {
        var candidate = sku;
        var n = 0;

        while (await db.Products.IgnoreQueryFilters().AnyAsync(p => p.Sku == candidate, cancellationToken))
        {
            n++;
            var suffix = $"-{n}";
            var combined = sku + suffix;
            candidate = combined.Length > 64 ? combined[..64] : combined;
        }

        return candidate;
    }
}
