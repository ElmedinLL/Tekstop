using FluentValidation;
using TechVault.API.Carts;

namespace TechVault.API.Validation.Carts;

public sealed class AddToCartDtoValidator : AbstractValidator<AddToCartDto>
{
    public AddToCartDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
