using System.Text.Json;
using AutoMapper;
using TechVault.API.Models;
using TechVault.API.Products;

namespace TechVault.API.Mapping;

public sealed class ProductMappingProfile : Profile
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    public ProductMappingProfile()
    {
        CreateMap<Category, ProductCategoryDto>();

        CreateMap<Product, ProductDto>()
            .ForMember(d => d.Stock, o => o.MapFrom(s => s.StockQuantity))
            .ForMember(d => d.Images, o => o.MapFrom(s => ExtractImages(s)))
            .ForMember(d => d.Specs, o => o.MapFrom(s => DeserializeSpecs(s.SpecsJson)))
            .ForMember(d => d.Category, o => o.MapFrom(s => s.Category));

        CreateMap<Product, ProductListDto>()
            .ForMember(d => d.Stock, o => o.MapFrom(s => s.StockQuantity))
            .ForMember(
                d => d.Description,
                o => o.MapFrom(s => s.ShortDescription ?? TruncateForList(s.Description)))
            .ForMember(d => d.Images, o => o.MapFrom(s => ExtractImages(s)))
            .ForMember(d => d.Specs, o => o.MapFrom(s => DeserializeSpecs(s.SpecsJson)))
            .ForMember(d => d.Category, o => o.MapFrom(s => s.Category));

        CreateMap<CreateProductDto, Product>()
            .ForMember(d => d.Category, o => o.Ignore())
            .ForMember(d => d.StockQuantity, o => o.MapFrom(s => s.Stock))
            .ForMember(d => d.ImagesJson, o => o.MapFrom(s => SerializeImages(s.Images)))
            .ForMember(d => d.SpecsJson, o => o.MapFrom(s => SerializeSpecs(s.Specs)))
            .AfterMap(
                (src, dest) =>
                {
                    dest.CreatedAtUtc = DateTime.UtcNow;
                    if (string.IsNullOrEmpty(dest.ImageUrl) && src.Images.Count > 0)
                    {
                        dest.ImageUrl = src.Images[0];
                    }
                });

        CreateMap<UpdateProductDto, Product>()
            .ForMember(d => d.Category, o => o.Ignore())
            .ForMember(d => d.Name, o => o.Condition(s => s.Name != null))
            .ForMember(d => d.Slug, o => o.Condition(s => s.Slug != null))
            .ForMember(d => d.Sku, o => o.Condition(s => s.Sku != null))
            .ForMember(d => d.Description, o => o.Condition(s => s.Description != null))
            .ForMember(
                d => d.Price,
                o =>
                {
                    o.Condition(s => s.Price.HasValue);
                    o.MapFrom(s => s.Price!.Value);
                })
            .ForMember(
                d => d.StockQuantity,
                o =>
                {
                    o.Condition(s => s.Stock.HasValue);
                    o.MapFrom(s => s.Stock!.Value);
                })
            .ForMember(
                d => d.CategoryId,
                o =>
                {
                    o.Condition(s => s.CategoryId.HasValue);
                    o.MapFrom(s => s.CategoryId!.Value);
                })
            .ForMember(
                d => d.ImagesJson,
                o =>
                {
                    o.Condition(s => s.Images != null);
                    o.MapFrom(s => SerializeImages(s.Images!));
                })
            .ForMember(
                d => d.SpecsJson,
                o =>
                {
                    o.Condition(s => s.Specs != null);
                    o.MapFrom(s => SerializeSpecs(s.Specs!));
                })
            .AfterMap(
                (src, dest) =>
                {
                    dest.UpdatedAtUtc = DateTime.UtcNow;
                    if (src.Images is null)
                    {
                        return;
                    }

                    if (src.Images.Count == 0)
                    {
                        dest.ImageUrl = null;
                        return;
                    }

                    dest.ImageUrl = src.Images[0];
                });
    }

    private static string? TruncateForList(string? description)
    {
        if (string.IsNullOrEmpty(description))
        {
            return description;
        }

        const int maxLen = 320;
        if (description.Length <= maxLen)
        {
            return description;
        }

        // Reserve one char for U+2026 so total length stays within maxLen.
        return description[..(maxLen - 1)] + "…";
    }

    private static List<string> ExtractImages(Product product)
    {
        // Prefer relational images (new storage) when present.
        if (product.Images is { Count: > 0 })
        {
            return product.Images
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.Id)
                .Select(i => i.Url)
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        // Fallback to legacy storage (single ImageUrl + ImagesJson array).
        return MergeLegacyImageUrl(product.ImageUrl, DeserializeImages(product.ImagesJson));
    }

    private static List<string> MergeLegacyImageUrl(string? imageUrl, List<string> fromJson)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            return fromJson;
        }

        if (fromJson.Count == 0)
        {
            return new List<string> { imageUrl };
        }

        if (fromJson.Contains(imageUrl))
        {
            return fromJson;
        }

        var merged = new List<string> { imageUrl };
        merged.AddRange(fromJson);
        return merged;
    }

    private static List<string> DeserializeImages(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<string>();
        }

        return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? new List<string>();
    }

    private static Dictionary<string, string> DeserializeSpecs(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<string, string>();
        }

        return JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions)
            ?? new Dictionary<string, string>();
    }

    private static string SerializeImages(List<string> images) =>
        JsonSerializer.Serialize(images, JsonOptions);

    private static string SerializeSpecs(Dictionary<string, string> specs) =>
        JsonSerializer.Serialize(specs, JsonOptions);
}
