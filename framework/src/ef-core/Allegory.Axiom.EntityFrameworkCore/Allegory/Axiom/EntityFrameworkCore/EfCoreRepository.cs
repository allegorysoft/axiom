using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

public class EfCoreRepository<TDbContext, TEntity>(
    IDbContextProvider<TDbContext> dbContextProvider)
    : IRepository<TEntity>
    where TDbContext : DbContext
    where TEntity : class, IEntity
{
    public static bool IsTenantEntity { get; }

    static EfCoreRepository()
    {
        IsTenantEntity = typeof(TEntity).IsAssignableFrom(typeof(ITenantOwned));
    }

    protected IDbContextProvider<TDbContext> DbContextProvider { get; } = dbContextProvider;

    protected virtual ValueTask<TDbContext> GetDbContextAsync(CancellationToken cancellationToken = default)
    {
        return DbContextProvider.GetAsync(cancellationToken);
    }

    protected virtual DbSet<TEntity> IncludeDetails(DbSet<TEntity> dbSet, bool includeDetails = true)
    {
        return dbSet;
    }

    public virtual async Task<TEntity> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        var context = await GetDbContextAsync(cancellationToken);
        var set = context.Set<TEntity>();
        set = IncludeDetails(set, includeDetails);

        return await set.FirstAsync(predicate, cancellationToken);
    }

    // We can use GetOrDefault instead FindAsync
    // DbSet<>.Find completely different  
    public virtual Task<TEntity?> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public virtual Task<IReadOnlyList<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? sort = null,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public virtual Task<IReadOnlyList<TEntity>> GetPagedListAsync(
        int skipCount, int maxResultCount,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? sort = null,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public virtual Task<long> GetCountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}