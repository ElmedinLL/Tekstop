using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechVault.API.Orders;
using TechVault.API.Services;

namespace TechVault.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public sealed class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderDto>> Create(
        [FromBody] CreateOrderDto dto,
        CancellationToken cancellationToken)
    {
        var identityUserId = ResolveIdentityUserId();
        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        try
        {
            var order = await orderService.CreateOrderAsync(identityUserId, dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { orderId = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(OrderListResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderListResult>> List(
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default)
    {
        var identityUserId = ResolveIdentityUserId();
        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        var result = await orderService.GetOrdersAsync(identityUserId, page, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{orderId:int}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderDto>> GetById(int orderId, CancellationToken cancellationToken)
    {
        var identityUserId = ResolveIdentityUserId();
        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        var order = await orderService.GetOrderByIdAsync(identityUserId, orderId, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    [HttpPost("{orderId:int}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Cancel(int orderId, CancellationToken cancellationToken)
    {
        var identityUserId = ResolveIdentityUserId();
        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        try
        {
            await orderService.CancelOrderAsync(identityUserId, orderId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private string? ResolveIdentityUserId()
        => User.FindFirstValue("UserId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
}
