using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechVault.API.Products;
using TechVault.API.Services;

namespace TechVault.API.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "Admin")]
public sealed class AdminProductsController(ILowStockInventoryService lowStockInventoryService) : ControllerBase
{
    /// <summary>Products with stock quantity below the configured low-stock threshold (default 10).</summary>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(IReadOnlyList<LowStockProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<LowStockProductDto>>> GetLowStock(CancellationToken cancellationToken)
    {
        var items = await lowStockInventoryService.GetLowStockProductsAsync(cancellationToken);
        return Ok(items);
    }
}
