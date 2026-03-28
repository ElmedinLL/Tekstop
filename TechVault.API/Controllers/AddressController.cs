using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechVault.API.Addresses;
using TechVault.API.Data;
using TechVault.API.Models;
using TechVault.API.Services;

namespace TechVault.API.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/addresses")]
[Authorize]
public sealed class AddressController(
    ApplicationDbContext db,
    IDomainUserService domainUserService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AddressDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AddressDto>>> List(CancellationToken cancellationToken)
    {
        var identityUserId = ResolveIdentityUserId();
        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        var domainUserId = await domainUserService.GetOrCreateDomainUserIdAsync(identityUserId, cancellationToken);
        var rows = await db.Addresses
            .AsNoTracking()
            .Where(a => a.UserId == domainUserId)
            .OrderByDescending(a => a.IsDefaultShipping)
            .ThenByDescending(a => a.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return Ok(rows.ConvertAll(ToDto));
    }

    [HttpPost]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AddressDto>> Create(
        [FromBody] CreateAddressDto dto,
        CancellationToken cancellationToken)
    {
        var identityUserId = ResolveIdentityUserId();
        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        var domainUserId = await domainUserService.GetOrCreateDomainUserIdAsync(identityUserId, cancellationToken);

        if (dto.IsDefaultShipping || dto.IsDefaultBilling)
        {
            await ClearDefaultsAsync(domainUserId, dto.IsDefaultShipping, dto.IsDefaultBilling, cancellationToken);
        }

        var entity = new Address
        {
            UserId = domainUserId,
            Label = dto.Label.Trim(),
            FullName = dto.FullName.Trim(),
            Line1 = dto.Line1.Trim(),
            Line2 = string.IsNullOrWhiteSpace(dto.Line2) ? null : dto.Line2.Trim(),
            City = dto.City.Trim(),
            Region = string.IsNullOrWhiteSpace(dto.Region) ? null : dto.Region.Trim(),
            PostalCode = dto.PostalCode.Trim(),
            Country = dto.Country.Trim(),
            Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim(),
            IsDefaultShipping = dto.IsDefaultShipping,
            IsDefaultBilling = dto.IsDefaultBilling,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Addresses.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return Created($"/api/addresses/{entity.Id}", ToDto(entity));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AddressDto>> Update(
        int id,
        [FromBody] UpdateAddressDto dto,
        CancellationToken cancellationToken)
    {
        var identityUserId = ResolveIdentityUserId();
        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        var domainUserId = await domainUserService.GetOrCreateDomainUserIdAsync(identityUserId, cancellationToken);
        var entity = await db.Addresses.FirstOrDefaultAsync(
            a => a.Id == id && a.UserId == domainUserId,
            cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        if (dto.IsDefaultShipping || dto.IsDefaultBilling)
        {
            await ClearDefaultsAsync(domainUserId, dto.IsDefaultShipping, dto.IsDefaultBilling, cancellationToken, exceptAddressId: id);
        }

        entity.Label = dto.Label.Trim();
        entity.FullName = dto.FullName.Trim();
        entity.Line1 = dto.Line1.Trim();
        entity.Line2 = string.IsNullOrWhiteSpace(dto.Line2) ? null : dto.Line2.Trim();
        entity.City = dto.City.Trim();
        entity.Region = string.IsNullOrWhiteSpace(dto.Region) ? null : dto.Region.Trim();
        entity.PostalCode = dto.PostalCode.Trim();
        entity.Country = dto.Country.Trim();
        entity.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        entity.IsDefaultShipping = dto.IsDefaultShipping;
        entity.IsDefaultBilling = dto.IsDefaultBilling;

        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var identityUserId = ResolveIdentityUserId();
        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        var domainUserId = await domainUserService.GetOrCreateDomainUserIdAsync(identityUserId, cancellationToken);
        var entity = await db.Addresses.FirstOrDefaultAsync(
            a => a.Id == id && a.UserId == domainUserId,
            cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        db.Addresses.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:int}/default")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AddressDto>> SetDefault(int id, CancellationToken cancellationToken)
    {
        var identityUserId = ResolveIdentityUserId();
        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        var domainUserId = await domainUserService.GetOrCreateDomainUserIdAsync(identityUserId, cancellationToken);
        var entity = await db.Addresses.FirstOrDefaultAsync(
            a => a.Id == id && a.UserId == domainUserId,
            cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        await ClearDefaultsAsync(domainUserId, true, true, cancellationToken, exceptAddressId: id);
        entity.IsDefaultShipping = true;
        entity.IsDefaultBilling = true;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    private async Task ClearDefaultsAsync(
        int domainUserId,
        bool clearShipping,
        bool clearBilling,
        CancellationToken cancellationToken,
        int? exceptAddressId = null)
    {
        var query = db.Addresses.Where(a => a.UserId == domainUserId);
        if (exceptAddressId is { } ex)
        {
            query = query.Where(a => a.Id != ex);
        }

        var list = await query.ToListAsync(cancellationToken);
        foreach (var a in list)
        {
            if (clearShipping)
            {
                a.IsDefaultShipping = false;
            }

            if (clearBilling)
            {
                a.IsDefaultBilling = false;
            }
        }
    }

    private static AddressDto ToDto(Address a) =>
        new()
        {
            Id = a.Id,
            Label = a.Label,
            FullName = a.FullName,
            Line1 = a.Line1,
            Line2 = a.Line2,
            City = a.City,
            Region = a.Region,
            PostalCode = a.PostalCode,
            Country = a.Country,
            Phone = a.Phone,
            IsDefaultShipping = a.IsDefaultShipping,
            IsDefaultBilling = a.IsDefaultBilling,
            CreatedAtUtc = a.CreatedAtUtc
        };

    private string? ResolveIdentityUserId()
        => User.FindFirstValue("UserId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
}
