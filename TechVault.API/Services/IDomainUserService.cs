namespace TechVault.API.Services;

public interface IDomainUserService
{
    /// <summary>
    /// Returns the catalog user id for this Identity user, creating a row on first use.
    /// </summary>
    Task<int> GetOrCreateDomainUserIdAsync(string identityUserId, CancellationToken cancellationToken = default);
}
