using System.Linq.Expressions;

namespace TechVault.API.Repositories;

/// <summary>
/// Generic read/write repository abstraction over EF Core <c>DbSet&lt;TEntity&gt;</c>.
/// Use <see cref="Query"/> for includes, filters, and projections. Composite keys pass all values in order (e.g. ProductTag: productId, tagId).
/// </summary>
public interface IRepository<TEntity> where TEntity : class
{
    IQueryable<TEntity> Query();

    IQueryable<TEntity> QueryAsNoTracking();

    ValueTask<TEntity?> FindAsync(object[] keyValues, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    void Remove(TEntity entity);

    void RemoveRange(IEnumerable<TEntity> entities);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
