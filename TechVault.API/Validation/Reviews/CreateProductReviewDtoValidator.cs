using FluentValidation;
using TechVault.API.Reviews;

namespace TechVault.API.Validation.Reviews;

public sealed class CreateProductReviewDtoValidator : AbstractValidator<CreateProductReviewDto>
{
    public CreateProductReviewDtoValidator()
    {
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).MaximumLength(4000);
        RuleFor(x => x.Title).MaximumLength(200);
    }
}
