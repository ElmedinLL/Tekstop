using FluentValidation;
using TechVault.API.Auth;

namespace TechVault.API.Validation.Auth;

public sealed class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
{
    public RefreshTokenDtoValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().MinimumLength(32).MaximumLength(512);
    }
}
