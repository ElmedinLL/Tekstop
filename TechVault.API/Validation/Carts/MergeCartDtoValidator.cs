using FluentValidation;
using TechVault.API.Carts;

namespace TechVault.API.Validation.Carts;

public sealed class MergeCartDtoValidator : AbstractValidator<MergeCartDto>
{
    public MergeCartDtoValidator()
    {
        RuleForEach(x => x.Lines).SetValidator(new MergeCartLineDtoValidator());
    }
}

public sealed class MergeCartLineDtoValidator : AbstractValidator<MergeCartLineDto>
{
    public MergeCartLineDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
