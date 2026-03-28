using FluentValidation;
using TechVault.API.Categories;

namespace TechVault.API.Validation.Categories;

public sealed class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Slug).MaximumLength(96);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.ImageUrl).MaximumLength(2048);
    }
}
