using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechVault.API.Admin;
using TechVault.API.Services;
using TechVault.API.Users;

namespace TechVault.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public sealed class AdminUsersController(IAdminUserService adminUserService) : ControllerBase
{
    /// <summary>Lists Identity users with roles, join date, order count, and lockout (ban) state.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(AdminUserListResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminUserListResult>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await adminUserService.ListUsersAsync(page, pageSize, search, cancellationToken);
        return Ok(result);
    }

    /// <summary>Sets account lockout (ban). Uses Identity lockout; cannot target admins or yourself.</summary>
    [HttpPut("{userId}/ban")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SetBanned(
        string userId,
        [FromBody] SetUserBannedDto dto,
        CancellationToken cancellationToken)
    {
        var adminId = User.FindFirstValue("UserId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        try
        {
            await adminUserService.SetUserBannedAsync(userId, dto.Banned, adminId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
