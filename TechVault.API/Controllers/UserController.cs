using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechVault.API.Auth;
using TechVault.API.Data;
using TechVault.API.Users;

namespace TechVault.API.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/users")]
[Authorize]
public sealed class UserController(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext db,
    IWebHostEnvironment webHostEnvironment) : ControllerBase
{
    private static readonly HashSet<string> AllowedImageExtensions =
    [
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"
    ];

    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileDto>> GetProfile(CancellationToken cancellationToken)
    {
        var identityUserId = ResolveIdentityUserId();
        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        var user = await userManager.FindByIdAsync(identityUserId);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(ToProfileDto(user));
    }

    [HttpPut("profile")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile(
        [FromBody] UpdateUserProfileDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var identityUserId = ResolveIdentityUserId();
        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        var user = await userManager.FindByIdAsync(identityUserId);
        if (user is null)
        {
            return NotFound();
        }

        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
            {
                ModelState.AddModelError(err.Code, err.Description);
            }

            return ValidationProblem(ModelState);
        }

        await SyncDomainUserAsync(identityUserId, user.FirstName, user.LastName, user.PhoneNumber, cancellationToken);

        return Ok(ToProfileDto(user));
    }

    /// <summary>Verifies the current password with Identity and sets a new password.</summary>
    [HttpPut("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var identityUserId = ResolveIdentityUserId();
        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        var user = await userManager.FindByIdAsync(identityUserId);
        if (user is null)
        {
            return NotFound();
        }

        var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
            {
                ModelState.AddModelError(err.Code, err.Description);
            }

            return ValidationProblem(ModelState);
        }

        return NoContent();
    }

    /// <summary>Upload a profile image. Stored under wwwroot/images/avatars.</summary>
    [HttpPost("profile/avatar")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UserAvatarUploadResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserAvatarUploadResponseDto>> UploadAvatar(
        [FromForm] IFormFile? file,
        CancellationToken cancellationToken)
    {
        file ??= Request.Form.Files.FirstOrDefault();
        if (file is null || file.Length == 0)
        {
            return BadRequest("A non-empty image file is required.");
        }

        var identityUserId = ResolveIdentityUserId();
        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        var user = await userManager.FindByIdAsync(identityUserId);
        if (user is null)
        {
            return NotFound();
        }

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !AllowedImageExtensions.Contains(ext.ToLowerInvariant()))
        {
            return BadRequest("Allowed image types: JPEG, PNG, GIF, WebP, BMP.");
        }

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("The uploaded file must be an image.");
        }

        var webRoot = webHostEnvironment.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
        {
            webRoot = Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");
        }

        var avatarsDir = Path.Combine(webRoot, "images", "avatars");
        Directory.CreateDirectory(avatarsDir);

        TryDeleteAvatarFile(webRoot, user.ProfilePicture);

        var safeId = SanitizePathSegment(identityUserId);
        var fileName = $"avatar-{safeId}-{Guid.NewGuid():N}{ext}";
        var physicalPath = Path.Combine(avatarsDir, fileName);

        await using (var stream = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                       bufferSize: 64 * 1024, useAsync: true))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var publicUrl = "/images/avatars/" + fileName;
        user.ProfilePicture = publicUrl;
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            TryDeletePhysicalFile(physicalPath);
            foreach (var err in updateResult.Errors)
            {
                ModelState.AddModelError(err.Code, err.Description);
            }

            return ValidationProblem(ModelState);
        }

        return Ok(new UserAvatarUploadResponseDto { AvatarUrl = publicUrl });
    }

    private static UserProfileDto ToProfileDto(ApplicationUser user) =>
        new()
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.ProfilePicture
        };

    private async Task SyncDomainUserAsync(
        string identityUserId,
        string firstName,
        string lastName,
        string? phone,
        CancellationToken cancellationToken)
    {
        var domainUser = await db.Users.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId, cancellationToken);
        if (domainUser is null)
        {
            return;
        }

        domainUser.FirstName = firstName;
        domainUser.LastName = lastName;
        domainUser.PhoneNumber = phone;
        domainUser.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private static void TryDeleteAvatarFile(string webRoot, string? profilePictureUrl)
    {
        if (string.IsNullOrEmpty(profilePictureUrl)
            || !profilePictureUrl.StartsWith("/images/avatars/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var relative = profilePictureUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var full = Path.GetFullPath(Path.Combine(webRoot, relative));
        var rootFull = Path.GetFullPath(webRoot);
        if (!full.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        TryDeletePhysicalFile(full);
    }

    private static void TryDeletePhysicalFile(string path)
    {
        try
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
        catch
        {
            // Best-effort cleanup
        }
    }

    private static string SanitizePathSegment(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (invalid.Contains(chars[i]))
            {
                chars[i] = '_';
            }
        }

        return new string(chars);
    }

    private string? ResolveIdentityUserId()
        => User.FindFirstValue("UserId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
}
