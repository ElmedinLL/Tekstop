using TechVault.API.Admin;

namespace TechVault.API.Services;

public interface IAdminUserService
{
    Task<AdminUserListResult> ListUsersAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default);

    Task SetUserBannedAsync(string identityUserId, bool banned, string currentAdminIdentityId, CancellationToken cancellationToken = default);
}
