using FluentValidation;
using TechVault.API.Models.Enums;
using TechVault.API.Orders;

namespace TechVault.API.Validation.Orders;

public sealed class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
{
    public UpdateOrderStatusDtoValidator()
    {
        RuleFor(x => x.Status).NotEmpty();

        RuleFor(x => x.TrackingUrl)
            .Must(HttpUrlValidation.IsNullOrValidHttpOrHttpsUrl)
            .WithMessage("Tracking URL must be a valid http or https URL.");

        When(
            x => Enum.TryParse<OrderStatus>(x.Status?.Trim(), ignoreCase: true, out var s) && s == OrderStatus.Shipped,
            () =>
            {
                RuleFor(x => x.TrackingUrl)
                    .NotEmpty()
                    .WithMessage("Tracking URL is required when status is Shipped.");
            });
    }
}
