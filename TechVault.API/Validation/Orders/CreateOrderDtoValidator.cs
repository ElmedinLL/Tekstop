using FluentValidation;
using TechVault.API.Orders;

namespace TechVault.API.Validation.Orders;

public sealed class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.AddressId).GreaterThan(0);
        RuleFor(x => x.PaymentMethod).NotEmpty().MaximumLength(32);
        RuleFor(x => x.CouponCode).MaximumLength(64);
        RuleFor(x => x.ShippingMethod)
            .NotEmpty()
            .Must(m => m.Equals("standard", StringComparison.OrdinalIgnoreCase)
                       || m.Equals("express", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Shipping method must be 'standard' or 'express'.");

        RuleFor(x => x.CartItems).NotEmpty();
        RuleForEach(x => x.CartItems).SetValidator(new CreateOrderCartItemDtoValidator());
    }
}

public sealed class CreateOrderCartItemDtoValidator : AbstractValidator<CreateOrderCartItemDto>
{
    public CreateOrderCartItemDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
