using TechVault.API.Coupons;

namespace TechVault.API.Services;

public interface ICouponValidationService
{
    Task<ValidateCouponResponseDto> ValidateAsync(string code, decimal orderSubtotal, CancellationToken cancellationToken = default);
}
