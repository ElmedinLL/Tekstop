using FluentValidation;
using TechVault.API.Auth;

namespace TechVault.API.Validation.Auth;

public sealed class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(256).EmailAddress();
        RuleFor(x => x.FirstName).NotEmpty().MinimumLength(2).MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MinimumLength(2).MaximumLength(50);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .Equal(x => x.Password)
            .WithMessage("Passwords do not match.");
    }
}
