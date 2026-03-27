using FluentValidation;
using TechVault.API.Carts;

namespace TechVault.API.Validation.Carts;

public sealed class UpdateCartDtoValidator : AbstractValidator<UpdateCartDto>
{
    public UpdateCartDtoValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
