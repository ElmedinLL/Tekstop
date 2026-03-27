using FluentValidation;
using TechVault.API.Orders;

namespace TechVault.API.Validation.Orders;

public sealed class ShipOrderDtoValidator : AbstractValidator<ShipOrderDto>
{
    public ShipOrderDtoValidator()
    {
        RuleFor(x => x.TrackingUrl)
            .NotEmpty()
            .Must(HttpUrlValidation.IsValidHttpOrHttpsUrl)
            .WithMessage("Tracking URL must be a valid http or https URL.");
    }
}
