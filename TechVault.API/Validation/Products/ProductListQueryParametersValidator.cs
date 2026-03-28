using FluentValidation;
using TechVault.API.Products;

namespace TechVault.API.Validation.Products;

public sealed class ProductListQueryParametersValidator : AbstractValidator<ProductListQueryParameters>
{
    public ProductListQueryParametersValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.MaxPrice)
            .Must((dto, max) => !dto.MinPrice.HasValue || !max.HasValue || max >= dto.MinPrice)
            .WithMessage("Max price must be greater than or equal to min price.");
    }
}
