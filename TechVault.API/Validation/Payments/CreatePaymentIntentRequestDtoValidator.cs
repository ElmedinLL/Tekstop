using FluentValidation;
using TechVault.API.Payments;

namespace TechVault.API.Validation.Payments;

public sealed class CreatePaymentIntentRequestDtoValidator : AbstractValidator<CreatePaymentIntentRequestDto>
{
    public CreatePaymentIntentRequestDtoValidator()
    {
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
