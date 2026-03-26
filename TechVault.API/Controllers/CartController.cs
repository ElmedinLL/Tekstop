using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechVault.API.Carts;
using TechVault.API.Services;

namespace TechVault.API.Controllers;

[ApiController]
[Route("api/cart")]
public sealed class CartController(ICartService cartService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartDto>> Get(CancellationToken cancellationToken)
    {
        var userId = await ResolveCartUserIdAsync(cancellationToken);
        return Ok(await cartService.GetCart(userId, cancellationToken));
    }

    [HttpPost("items")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CartDto>> Add(
        [FromBody] AddToCartDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = await ResolveCartUserIdAsync(cancellationToken);
            return Ok(await cartService.AddItem(userId, dto.ProductId, dto.Quantity, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("items/{itemId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CartDto>> UpdateQty(
        int itemId,
        [FromBody] UpdateCartDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = await ResolveCartUserIdAsync(cancellationToken);
            return Ok(await cartService.UpdateQty(userId, itemId, dto.Quantity, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("items/{itemId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CartDto>> RemoveItem(int itemId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = await ResolveCartUserIdAsync(cancellationToken);
            return Ok(await cartService.RemoveItem(userId, itemId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartDto>> Clear(CancellationToken cancellationToken)
    {
        var userId = await ResolveCartUserIdAsync(cancellationToken);
        return Ok(await cartService.ClearCart(userId, cancellationToken));
    }

    private async Task<string> ResolveCartUserIdAsync(CancellationToken cancellationToken)
    {
        var id = User.FindFirstValue("UserId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!string.IsNullOrEmpty(id))
        {
            return id;
        }

        await HttpContext.Session.LoadAsync(cancellationToken);
        var guest = HttpContext.Session.GetString("GuestCartUserId");
        if (string.IsNullOrEmpty(guest))
        {
            guest = "guest:" + Guid.NewGuid().ToString("N");
            HttpContext.Session.SetString("GuestCartUserId", guest);
            await HttpContext.Session.CommitAsync(cancellationToken);
        }

        return guest;
    }
}
