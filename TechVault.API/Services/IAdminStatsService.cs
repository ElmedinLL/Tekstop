using TechVault.API.Admin;

namespace TechVault.API.Services;

public interface IAdminStatsService
{
    Task<AdminStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
}
