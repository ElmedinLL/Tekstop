using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechVault.API.Orders;
using TechVault.API.Services;

namespace TechVault.API.Controllers;

[Authorize]
[ApiController]
[Route("api/orders")]
public sealed class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderDetailDto>> Create(
        [FromBody] CreateOrderDto dto,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var order = await orderService.CreateAsync(userId, dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<OrderListItemDto>>> List(CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        return Ok(await orderService.ListAsync(userId, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderDetailDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var order = await orderService.GetByIdAsync(userId, id, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    private string? ResolveUserId() =>
        User.FindFirstValue("UserId")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
}
