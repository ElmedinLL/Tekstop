using FluentValidation;
using TechVault.API.Products;

namespace TechVault.API.Validation.Products;

public sealed class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Slug)
            .MaximumLength(280)
            .Must((dto, slug) => !string.IsNullOrWhiteSpace(slug) || !string.IsNullOrWhiteSpace(dto.Name))
            .WithMessage(
                "Provide a non-empty slug, or omit it so a slug can be generated from the product name.");
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(64);
        RuleFor(x => x.ShortDescription).MaximumLength(500);
        RuleFor(x => x.Brand).MaximumLength(120);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.CompareAtPrice).GreaterThan(0).When(x => x.CompareAtPrice.HasValue);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleForEach(x => x.Images).MaximumLength(2048);
    }
}
