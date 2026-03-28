using FluentValidation;
using TechVault.API.Products;

namespace TechVault.API.Validation.Products;

public sealed class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .Must(n => !string.IsNullOrWhiteSpace(n))
            .MaximumLength(256)
            .When(x => x.Name is not null);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(280).When(x => x.Slug is not null);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(64).When(x => x.Sku is not null);
        RuleFor(x => x.ShortDescription).MaximumLength(500).When(x => x.ShortDescription is not null);
        RuleFor(x => x.Brand).MaximumLength(120).When(x => x.Brand is not null);
        RuleFor(x => x.Price).GreaterThan(0).When(x => x.Price.HasValue);
        RuleFor(x => x.CompareAtPrice).GreaterThan(0).When(x => x.CompareAtPrice.HasValue);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0).When(x => x.Stock.HasValue);
        RuleFor(x => x.CategoryId).GreaterThan(0).When(x => x.CategoryId.HasValue);
        RuleForEach(x => x.Images!).MaximumLength(2048).When(x => x.Images is not null);
    }
}
