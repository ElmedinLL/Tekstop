using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechVault.API.Coupons;
using TechVault.API.Services;

namespace TechVault.API.Controllers;

[ApiController]
[Route("api/coupons")]
public sealed class CouponController(ICouponValidationService couponValidationService) : ControllerBase
{
    [HttpPost("validate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ValidateCouponResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ValidateCouponResponseDto>> Validate(
        [FromBody] ValidateCouponDto dto,
        CancellationToken cancellationToken)
    {
        var result = await couponValidationService.ValidateAsync(dto.Code, dto.OrderSubtotal, cancellationToken);
        return Ok(result);
    }
}
