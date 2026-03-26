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
        return Ok(await cartService.GetAsync(cancellationToken));
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
            return Ok(await cartService.AddAsync(dto, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("items")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CartDto>> Update(
        [FromBody] UpdateCartDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await cartService.UpdateAsync(dto, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
