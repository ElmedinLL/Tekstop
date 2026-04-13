using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
    IWebHostEnvironment webHostEnvironment,
    ILogger<AuthController> logger)
    : ControllerBase
{
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] RefreshTokenDto? dto)
    {
        var userId = ResolveCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var tokenValue = dto?.RefreshToken ?? Request.Cookies[AuthCookieNames.Refresh];
        if (!string.IsNullOrWhiteSpace(tokenValue))
        {
            var refreshToken = await authDbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == tokenValue && rt.UserId == userId);

            if (refreshToken != null && !refreshToken.IsRevoked)
            {
                refreshToken.RevokedAtUtc = DateTime.UtcNow;
                await authDbContext.SaveChangesAsync();
            }
        }

        DeleteRefreshCookie();

        // Idempotent: do not leak whether a token existed.
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserProfileDto>> Me()
    {
        var userId = ResolveCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        // Always resolve roles from Identity (authoritative). JWT role claims can be missing or mapped
        // differently depending on JwtBearer / claim mapping, which breaks the SPA admin check.
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(user);

        return Ok(new CurrentUserProfileDto
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ProfilePicture = user.ProfilePicture,
            Roles = roles.ToArray()
        });
    }

    /// <summary>Lets the SPA skip POST /auth/refresh when there is no refresh cookie and no stored token (reduces anonymous 401 noise).</summary>
    [HttpGet("session")]
    public ActionResult<AuthSessionBootstrapDto> SessionBootstrap()
    {
        var raw = Request.Cookies[AuthCookieNames.Refresh];
        var hasCookie = !string.IsNullOrWhiteSpace(raw);
        return Ok(new AuthSessionBootstrapDto { HasRefreshCookie = hasCookie });
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
    public async Task<ActionResult<AuthResponseDto>> Refresh(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] RefreshTokenDto? dto)
    {
        var tokenValue = Request.Cookies[AuthCookieNames.Refresh] ?? dto?.RefreshToken;
        if (string.IsNullOrWhiteSpace(tokenValue))
        {
            return Unauthorized();
        }

        var refreshToken = await authDbContext.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.Token == tokenValue);

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
        return await authDbContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
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

                var authResponse = new AuthResponseDto
                {
                    AccessToken = jwt.AccessToken,
                    AccessTokenExpiresAtUtc = jwt.AccessTokenExpiresAtUtc,
                    RefreshToken = jwt.RefreshToken
                };
                AppendRefreshCookie(jwt.RefreshToken);
                return Ok(authResponse);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        });
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

        var result = new AuthResponseDto
        {
            AccessToken = jwt.AccessToken,
            AccessTokenExpiresAtUtc = jwt.AccessTokenExpiresAtUtc,
            RefreshToken = jwt.RefreshToken
        };
        AppendRefreshCookie(jwt.RefreshToken);
        return result;
    }

    private CookieOptions BuildRefreshCookieOptions()
    {
        var secure = webHostEnvironment.IsProduction() || Request.IsHttps;
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Lax,
            Path = "/api/v1",
            MaxAge = TimeSpan.FromDays(30),
            IsEssential = true,
        };
    }

    private void AppendRefreshCookie(string refreshTokenValue)
    {
        Response.Cookies.Append(AuthCookieNames.Refresh, refreshTokenValue, BuildRefreshCookieOptions());
    }

    private void DeleteRefreshCookie()
    {
        var secure = webHostEnvironment.IsProduction() || Request.IsHttps;
        Response.Cookies.Delete(AuthCookieNames.Refresh, new CookieOptions
        {
            Path = "/api/v1",
            SameSite = SameSiteMode.Lax,
            Secure = secure,
            HttpOnly = true,
        });
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

