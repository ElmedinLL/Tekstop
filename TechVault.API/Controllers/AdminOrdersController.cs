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

    [HttpGet("{orderId:int}")]
    [ProducesResponseType(typeof(AdminOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminOrderDetailDto>> GetById(int orderId, CancellationToken cancellationToken)
    {
        var order = await adminOrderService.GetOrderByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    /// <summary>Updates order status. Use status Shipped with a tracking URL to trigger the shipped email (same rules as POST …/ship).</summary>
    [HttpPut("{orderId:int}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateStatus(
        int orderId,
        [FromBody] UpdateOrderStatusDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            await adminOrderService.UpdateOrderStatusAsync(orderId, dto, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("was not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }

            return BadRequest(ex.Message);
        }
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
            if (ex.Message.Contains("was not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }

            return BadRequest(ex.Message);
        }
    }
}
