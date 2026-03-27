using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechVault.API.Models.Enums;
using TechVault.API.Orders;
using TechVault.API.Services;

namespace TechVault.API.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Admin")]
public sealed class AdminOrdersController(IAdminOrderService adminOrderService) : ControllerBase
{
    /// <summary>
    /// Lists orders with optional filters: status, placed date range (UTC calendar days), search (numeric id, order number, or email substring).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(AdminOrderListResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminOrderListResult>> List(
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        OrderStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var parsed))
            {
                return BadRequest("Invalid status. Use a value like Pending, Paid, Shipped, Delivered, Cancelled, etc.");
            }

            statusFilter = parsed;
        }

        var result = await adminOrderService.ListOrdersAsync(
            statusFilter,
            from,
            to,
            search,
            page,
            pageSize,
            cancellationToken);

        return Ok(result);
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
