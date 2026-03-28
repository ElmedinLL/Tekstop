using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechVault.API.Data;
using TechVault.API.Auth;
using TechVault.API.Models.Enums;
using TechVault.API.Services;

namespace TechVault.API.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IJwtTokenService jwtTokenService,
    AuthDbContext authDbContext,
    IDomainUserService domainUserService,
    ILogger<AuthController> logger)
    : ControllerBase
{
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
    {
        var userId = ResolveCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var refreshToken = await authDbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken && rt.UserId == userId);

        if (refreshToken != null && !refreshToken.IsRevoked)
        {
            refreshToken.RevokedAtUtc = DateTime.UtcNow;
            await authDbContext.SaveChangesAsync();
        }

        // Idempotent: do not leak whether a token existed.
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<CurrentUserProfileDto> Me()
    {
        var userId = ResolveCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var email = User.FindFirstValue("Email")
            ?? User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
            ?? string.Empty;

        var firstName = User.FindFirstValue("FirstName") ?? string.Empty;
        var lastName = User.FindFirstValue("LastName") ?? string.Empty;
        var profilePicture = User.FindFirstValue("ProfilePicture");

        var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "Roles")
            .Select(c => c.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return Ok(new CurrentUserProfileDto
        {
            UserId = userId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            ProfilePicture = profilePicture,
            Roles = roles
        });
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        ApplicationUser? user = null;
        try
        {
            user = new ApplicationUser
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
                    await RollbackFailedRegistrationAsync(user);
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

                await RollbackFailedRegistrationAsync(user);
                return ValidationProblem(ModelState);
            }

            logger.LogInformation("User registered: {UserId} {Email}", user.Id, user.Email);

            return Ok(await IssueTokensAndPersistRefreshAsync(user));
        }
        catch
        {
            if (user is not null)
            {
                await RollbackFailedRegistrationAsync(user);
            }

            throw;
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var email = dto.Email.Trim();
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return Unauthorized();
        }

        if (userManager.SupportsUserLockout)
        {
            var lockoutEnd = user.LockoutEnd;
            if (lockoutEnd.HasValue && lockoutEnd.Value.UtcDateTime > DateTime.UtcNow)
            {
                return StatusCode(StatusCodes.Status423Locked);
            }
        }

        var passwordOk = await userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordOk)
        {
            if (userManager.SupportsUserLockout)
            {
                await userManager.AccessFailedAsync(user);
            }

            return Unauthorized();
        }

        if (userManager.SupportsUserLockout)
        {
            await userManager.ResetAccessFailedCountAsync(user);
        }

        logger.LogInformation("User login: {UserId} {Email}", user.Id, user.Email);

        return Ok(await IssueTokensAndPersistRefreshAsync(user));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenDto dto)
    {
        var refreshToken = await authDbContext.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.Token == dto.RefreshToken);

        if (refreshToken == null)
        {
            return Unauthorized();
        }

        if (refreshToken.IsRevoked)
        {
            return Unauthorized();
        }

        if (refreshToken.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return Unauthorized();
        }

        var userForRefresh = await userManager.FindByIdAsync(refreshToken.UserId);
        if (userForRefresh == null)
        {
            return Unauthorized();
        }

        await domainUserService.GetOrCreateDomainUserIdAsync(userForRefresh.Id);

        // Rotation strategy:
        // - revoke the used refresh token
        // - issue a fresh JWT pair + a brand new refresh token
        await using var tx = await authDbContext.Database.BeginTransactionAsync();

        try
        {
            refreshToken.RevokedAtUtc = DateTime.UtcNow;

            var jwt = await jwtTokenService.GenerateToken(userForRefresh);

            var newRefreshTokenLifetimeUtc = DateTime.UtcNow.AddDays(30);
            authDbContext.RefreshTokens.Add(new RefreshToken
            {
                UserId = userForRefresh.Id,
                Token = jwt.RefreshToken,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = newRefreshTokenLifetimeUtc
            });

            await authDbContext.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(new AuthResponseDto
            {
                AccessToken = jwt.AccessToken,
                AccessTokenExpiresAtUtc = jwt.AccessTokenExpiresAtUtc,
                RefreshToken = jwt.RefreshToken
            });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    private string? ResolveCurrentUserId()
        => User.FindFirstValue("UserId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

    private async Task<AuthResponseDto> IssueTokensAndPersistRefreshAsync(ApplicationUser user)
    {
        // Ensure catalog user exists before persisting refresh tokens so a failure does not leave
        // a half-registered Identity account (email taken) with no successful response.
        await domainUserService.GetOrCreateDomainUserIdAsync(user.Id);

        var jwt = await jwtTokenService.GenerateToken(user);
        var refreshTokenLifetimeUtc = DateTime.UtcNow.AddDays(30);

        authDbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = jwt.RefreshToken,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = refreshTokenLifetimeUtc
        });

        await authDbContext.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = jwt.AccessToken,
            AccessTokenExpiresAtUtc = jwt.AccessTokenExpiresAtUtc,
            RefreshToken = jwt.RefreshToken
        };
    }

    private async Task RollbackFailedRegistrationAsync(ApplicationUser user)
    {
        await domainUserService.DeleteByIdentityUserIdIfExistsAsync(user.Id);
        try
        {
            await userManager.DeleteAsync(user);
        }
        catch
        {
            // Best-effort cleanup; rethrow from caller if needed.
        }
    }
}

