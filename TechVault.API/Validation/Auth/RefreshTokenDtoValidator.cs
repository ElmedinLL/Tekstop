using FluentValidation;
using TechVault.API.Auth;

namespace TechVault.API.Validation.Auth;

public sealed class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
{
    public RefreshTokenDtoValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.RefreshToken), () =>
        {
            RuleFor(x => x.RefreshToken!).MinimumLength(32).MaximumLength(512);
        });
    }
}
