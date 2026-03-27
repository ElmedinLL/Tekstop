using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechVault.API.Orders;
using TechVault.API.Services;

namespace TechVault.API.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Admin")]
public sealed class AdminOrdersController(IAdminOrderService adminOrderService) : ControllerBase
{
    [HttpPost("{orderId:int}/ship")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Ship(
        int orderId,
        [FromBody] ShipOrderDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            await adminOrderService.MarkOrderShippedAsync(orderId, dto.TrackingUrl, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
