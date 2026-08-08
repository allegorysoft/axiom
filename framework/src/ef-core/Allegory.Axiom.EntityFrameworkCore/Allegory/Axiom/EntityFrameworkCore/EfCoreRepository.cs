using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.Exceptions;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

public class EfCoreRepository<TDbContext, TEntity>(
    IDbContextProvider<TDbContext> dbContextProvider)
    : IRepository<TEntity>
    where TDbContext : DbContext
    where TEntity : class, IEntity
{
    public static bool IsTenantOwned { get; }

    static EfCoreRepository()
    {
        IsTenantOwned = typeof(TEntity).IsAssignableFrom(typeof(ITenantOwned));
    }

    protected IDbContextProvider<TDbContext> DbContextProvider { get; } = dbContextProvider;
    protected IUnitOfWork UnitOfWork => DbContextProvider.UnitOfWorkManager.RequiredCurrent;
    // cancellationToken.FallbackTo(UnitOfWork.CancellationToken)

    protected virtual ValueTask<TDbContext> GetDbContextAsync(CancellationToken cancellationToken = default)
    {
        return DbContextProvider.GetAsync(cancellationToken);
    }

    protected virtual async ValueTask<DbSet<TEntity>> GetDbSetAsync(CancellationToken cancellationToken = default)
    {
        var context = await GetDbContextAsync(cancellationToken);
        return context.Set<TEntity>();
    }

    protected virtual IQueryable<TEntity> IncludeDetails(IQueryable<TEntity> query, bool includeDetails = true)
    {
        return query;
    }

    public virtual async Task<TEntity> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(predicate, includeDetails, cancellationToken);

        // Create EntityNotFoundException inside Domain package
        return entity ?? throw new NotFoundException();
    }

    public virtual async Task<TEntity?> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        var set = await GetDbSetAsync(cancellationToken);
        var query = set.AsQueryable();

        query = IncludeDetails(query, includeDetails);

        return await query.FirstOrDefaultAsync(predicate, cancellationToken: cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        var set = await GetDbSetAsync(cancellationToken);
        var query = set.AsQueryable();

        query = IncludeDetails(query, includeDetails);

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetPagedListAsync(
        int skip,
        int take,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy,
        Expression<Func<TEntity, bool>>? predicate = null, bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        var set = await GetDbSetAsync(cancellationToken);
        var query = set.AsQueryable();

        query = IncludeDetails(query, includeDetails);

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        query = orderBy(query);

        return await query.Skip(skip).Take(take).ToListAsync(cancellationToken);
    }

    public virtual async Task<long> GetCountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var set = await GetDbSetAsync(cancellationToken);

        if (predicate == null)
        {
            return await set.LongCountAsync(cancellationToken: cancellationToken);
        }

        return await set.LongCountAsync(predicate, cancellationToken);
    }

    public virtual async ValueTask<TEntity> AddAsync(
        TEntity entity,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        var set = await GetDbSetAsync(cancellationToken);

        var result = await set.AddAsync(entity, cancellationToken);

        if (autoSave)
        {
            // We use unit of work because database handle owns transaction begin
            await UnitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result.Entity;
    }

    public virtual async Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        var set = await GetDbSetAsync(cancellationToken);

        await set.AddRangeAsync(entities, cancellationToken);

        if (autoSave)
        {
            await UnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async ValueTask<TEntity> UpdateAsync(
        TEntity entity,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        var set = await GetDbSetAsync(cancellationToken);

        var result = set.Update(entity);

        if (autoSave)
        {
            await UnitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result.Entity;
    }

    public virtual async Task UpdateRangeAsync(
        IEnumerable<TEntity> entities,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        var set = await GetDbSetAsync(cancellationToken);

        set.UpdateRange(entities);

        if (autoSave)
        {
            await UnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task RemoveAsync(
        TEntity entity,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        var set = await GetDbSetAsync(cancellationToken);

        var result = set.Remove(entity);

        if (autoSave)
        {
            await UnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task RemoveRangeAsync(
        IEnumerable<TEntity> entities,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        var set = await GetDbSetAsync(cancellationToken);

        set.RemoveRange(entities);

        if (autoSave)
        {
            await UnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}

public class EfCoreRepository<TDbContext, TEntity, TKey>(
    IDbContextProvider<TDbContext> dbContextProvider) :
    EfCoreRepository<TDbContext, TEntity>(dbContextProvider),
    IRepository<TEntity, TKey>
    where TDbContext : DbContext
    where TEntity : class, IEntity<TKey>
    where TKey : notnull
{
    public virtual async Task<TEntity> GetAsync(
        TKey id,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(id, includeDetails, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException();
        }

        return entity;
    }

    public virtual Task<TEntity?> FindAsync(
        TKey id,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        return FindAsync(e => e.Id.Equals(id), includeDetails, cancellationToken);
    }

    public virtual async Task RemoveAsync(
        TKey id,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(id, cancellationToken: cancellationToken);

        if (entity == null)
        {
            return;
        }

        await RemoveAsync(entity, autoSave, cancellationToken);
    }

    public virtual async Task RemoveRangeAsync(
        IEnumerable<TKey> ids,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        var set = await GetDbSetAsync(cancellationToken);
        var entities = await set.Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);

        await RemoveRangeAsync(entities, autoSave, cancellationToken);
    }
}