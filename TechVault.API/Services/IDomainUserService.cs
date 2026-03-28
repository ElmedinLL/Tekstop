namespace TechVault.API.Services;

public interface IDomainUserService
{
    /// <summary>
    /// Returns the catalog user id for this Identity user, creating a row on first use.
    /// </summary>
    Task<int> GetOrCreateDomainUserIdAsync(string identityUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the catalog user row for this Identity id if present (e.g. rollback failed registration).
    /// </summary>
    Task DeleteByIdentityUserIdIfExistsAsync(string identityUserId, CancellationToken cancellationToken = default);
}
