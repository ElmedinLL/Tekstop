using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TechVault.API.Data;

namespace TechVault.API.Repositories;

public class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected ApplicationDbContext Context { get; }
    protected DbSet<TEntity> Set { get; }

    public BaseRepository(ApplicationDbContext context)
    {
        Context = context;
        Set = context.Set<TEntity>();
    }

    public virtual IQueryable<TEntity> Query() => Set.AsQueryable();

    public virtual IQueryable<TEntity> QueryAsNoTracking() => Set.AsNoTracking();

    public virtual ValueTask<TEntity?> FindAsync(object[] keyValues, CancellationToken cancellationToken = default)
        => Set.FindAsync(keyValues, cancellationToken);

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => await Set.AsNoTracking().ToListAsync(cancellationToken);

    public virtual Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => Set.AnyAsync(predicate, cancellationToken);

    public virtual Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
        => predicate is null ? Set.CountAsync(cancellationToken) : Set.CountAsync(predicate, cancellationToken);

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Set.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        => await Set.AddRangeAsync(entities, cancellationToken);

    public virtual void Update(TEntity entity) => Set.Update(entity);

    public virtual void Remove(TEntity entity) => Set.Remove(entity);

    public virtual void RemoveRange(IEnumerable<TEntity> entities) => Set.RemoveRange(entities);

    public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => Context.SaveChangesAsync(cancellationToken);
}
