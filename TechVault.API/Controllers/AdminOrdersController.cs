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
    [HttpGet]
    [ProducesResponseType(typeof(AdminOrderListResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminOrderListResult>> List(
        [FromQuery] int page = 1,
        [FromQuery] string? status = null,
        [FromQuery] string? from = null,
        [FromQuery] string? to = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await adminOrderService.ListOrdersAsync(page, status, from, to, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{orderId:int}")]
    [ProducesResponseType(typeof(AdminOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminOrderDetailDto>> GetById(int orderId, CancellationToken cancellationToken)
    {
        var order = await adminOrderService.GetOrderByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        return Ok(order);
    }

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
