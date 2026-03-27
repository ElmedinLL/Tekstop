using FluentValidation;
using TechVault.API.Payments;

namespace TechVault.API.Validation.Payments;

public sealed class ConfirmPaymentRequestDtoValidator : AbstractValidator<ConfirmPaymentRequestDto>
{
    public ConfirmPaymentRequestDtoValidator()
    {
        RuleFor(x => x.PaymentIntentId).NotEmpty();
        RuleFor(x => x.PaymentMethodId).NotEmpty();
    }
}
