using FluentValidation;
using TechVault.API.Coupons;
using TechVault.API.Models.Enums;

namespace TechVault.API.Validation.Coupons;

public abstract class AdminCouponWriteDtoValidatorBase<T> : AbstractValidator<T>
    where T : CreateAdminCouponDto
{
    protected AdminCouponWriteDtoValidatorBase()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.MinOrderValue).GreaterThanOrEqualTo(0);
        RuleFor(x => x.UsageLimit).GreaterThanOrEqualTo(0).When(x => x.UsageLimit.HasValue);

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .When(x => x.DiscountType == DiscountType.Percent)
            .WithMessage("Percent discount must be between 0 and 100.");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0)
            .When(x => x.DiscountType == DiscountType.Fixed)
            .WithMessage("Fixed discount must be greater than 0.");
    }
}

public sealed class CreateAdminCouponDtoValidator : AdminCouponWriteDtoValidatorBase<CreateAdminCouponDto>
{
}

public sealed class UpdateAdminCouponDtoValidator : AdminCouponWriteDtoValidatorBase<UpdateAdminCouponDto>
{
}
