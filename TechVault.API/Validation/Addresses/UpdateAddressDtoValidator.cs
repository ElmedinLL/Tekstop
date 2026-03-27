using FluentValidation;
using TechVault.API.Addresses;

namespace TechVault.API.Validation.Addresses;

public sealed class UpdateAddressDtoValidator : AbstractValidator<UpdateAddressDto>
{
    public UpdateAddressDtoValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(64);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Line1).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Line2).MaximumLength(256);
        RuleFor(x => x.City).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Region).MaximumLength(128);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Phone).MaximumLength(32);
    }
}
