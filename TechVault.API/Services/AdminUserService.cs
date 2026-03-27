using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechVault.API.Admin;
using TechVault.API.Auth;
using TechVault.API.Data;

namespace TechVault.API.Services;

public sealed class AdminUserService(
    UserManager<ApplicationUser> userManager,
    AuthDbContext authDb,
    ApplicationDbContext db) : IAdminUserService
{
    private const int MaxPageSize = 100;

    public async Task<AdminUserListResult> ListUsersAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = userManager.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            if (term.Length > 256)
            {
                term = term[..256];
            }

            query = query.Where(u =>
                (u.Email != null && u.Email.Contains(term)) ||
                u.FirstName.Contains(term) ||
                u.LastName.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (users.Count == 0)
        {
            return new AdminUserListResult
            {
                Items = Array.Empty<AdminUserListItemDto>(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        var identityIds = users.Select(u => u.Id).ToList();

        var domainMap = await db.Users.AsNoTracking()
            .Where(u => u.IdentityUserId != null && identityIds.Contains(u.IdentityUserId))
            .ToDictionaryAsync(u => u.IdentityUserId!, u => u.Id, cancellationToken);

        var domainIds = domainMap.Values.ToHashSet();

        var roleRows = await (
            from ur in authDb.Set<IdentityUserRole<string>>()
            join r in authDb.Roles on ur.RoleId equals r.Id
            where identityIds.Contains(ur.UserId)
            select new { ur.UserId, RoleName = r.Name! }
        ).ToListAsync(cancellationToken);

        var rolesByUser = roleRows
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName).OrderBy(x => x).ToArray());

        var orderRows = await db.Orders.AsNoTracking()
            .Where(o =>
                (o.IdentityUserId != null && identityIds.Contains(o.IdentityUserId)) ||
                (o.UserId != null && domainIds.Contains(o.UserId.Value)))
            .Select(o => new { o.IdentityUserId, o.UserId })
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var items = new List<AdminUserListItemDto>(users.Count);

        foreach (var u in users)
        {
            int? domainUserId = null;
            if (domainMap.TryGetValue(u.Id, out var duid))
            {
                domainUserId = duid;
            }

            var orderCount = orderRows.Count(o =>
                o.IdentityUserId == u.Id ||
                (domainUserId.HasValue && o.UserId == domainUserId.Value));

            var isBanned = u.LockoutEnd.HasValue && u.LockoutEnd.Value.UtcDateTime > now;

            rolesByUser.TryGetValue(u.Id, out var roles);
            roles ??= Array.Empty<string>();

            var joined = u.CreatedAt;
            if (joined.Kind == DateTimeKind.Unspecified)
            {
                joined = DateTime.SpecifyKind(joined, DateTimeKind.Utc);
            }
            else if (joined.Kind == DateTimeKind.Local)
            {
                joined = joined.ToUniversalTime();
            }

            items.Add(new AdminUserListItemDto
            {
                UserId = u.Id,
                Email = u.Email ?? string.Empty,
                FirstName = u.FirstName,
                LastName = u.LastName,
                AvatarUrl = u.ProfilePicture,
                Roles = roles,
                JoinedAtUtc = joined,
                OrderCount = orderCount,
                IsBanned = isBanned
            });
        }

        return new AdminUserListResult
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task SetUserBannedAsync(
        string identityUserId,
        bool banned,
        string currentAdminIdentityId,
        CancellationToken cancellationToken = default)
    {
        if (string.Equals(identityUserId, currentAdminIdentityId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("You cannot change the ban status for your own account.");
        }

        var user = await userManager.FindByIdAsync(identityUserId);
        if (user is null)
        {
            throw new KeyNotFoundException("User was not found.");
        }

        var roles = await userManager.GetRolesAsync(user);
        if (roles.Contains("Admin", StringComparer.Ordinal))
        {
            throw new InvalidOperationException("Administrator accounts cannot be banned.");
        }

        await userManager.SetLockoutEnabledAsync(user, true);
        if (banned)
        {
            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        }
        else
        {
            await userManager.SetLockoutEndDateAsync(user, null);
        }
    }
}
