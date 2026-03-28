using FluentValidation;
using TechVault.API.Coupons;

namespace TechVault.API.Validation.Coupons;

public sealed class ValidateCouponDtoValidator : AbstractValidator<ValidateCouponDto>
{
    public ValidateCouponDtoValidator()
    {
        RuleFor(x => x.Code).NotEmpty();
        RuleFor(x => x.OrderSubtotal).GreaterThanOrEqualTo(0);
    }
}
