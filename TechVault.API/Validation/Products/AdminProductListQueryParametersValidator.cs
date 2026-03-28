using FluentValidation;
using TechVault.API.Products;

namespace TechVault.API.Validation.Products;

public sealed class AdminProductListQueryParametersValidator : AbstractValidator<AdminProductListQueryParameters>
{
    public AdminProductListQueryParametersValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
