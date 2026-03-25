using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechVault.API.Auth;
using TechVault.API.Models.Enums;

namespace TechVault.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IJwtTokenService jwtTokenService)
    : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        var user = new ApplicationUser
        {
            Email = dto.Email,
            UserName = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName
        };

        var createResult = await userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return ValidationProblem(ModelState);
        }

        var customerRoleName = nameof(UserRole.Customer);
        if (!await roleManager.RoleExistsAsync(customerRoleName))
        {
            var roleCreateResult = await roleManager.CreateAsync(new IdentityRole(customerRoleName));
            if (!roleCreateResult.Succeeded)
            {
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Role creation failed",
                    detail: string.Join("; ", roleCreateResult.Errors.Select(e => e.Description)));
            }
        }

        var addRoleResult = await userManager.AddToRoleAsync(user, customerRoleName);
        if (!addRoleResult.Succeeded)
        {
            foreach (var error in addRoleResult.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return ValidationProblem(ModelState);
        }

        var jwt = await jwtTokenService.GenerateToken(user);
        return Ok(new AuthResponseDto
        {
            AccessToken = jwt.AccessToken,
            AccessTokenExpiresAtUtc = jwt.AccessTokenExpiresAtUtc,
            RefreshToken = jwt.RefreshToken
        });
    }
}

