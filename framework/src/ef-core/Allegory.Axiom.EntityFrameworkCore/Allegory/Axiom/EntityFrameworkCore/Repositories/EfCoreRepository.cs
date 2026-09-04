using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Data.ConnectionStrings;
using Allegory.Axiom.Data.Filtering;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.Domain.Entities.Auditing;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.EntityFrameworkCore.Repositories;

public class EfCoreRepository<TDbContext, TEntity> : IRepository<TEntity>
    where TDbContext : DbContext
    where TEntity : class, IEntity
{
    static EfCoreRepository()
    {
        IsTenantOwned = typeof(TEntity).IsAssignableFrom(typeof(ITenantOwned));
        IsSoftDelete = typeof(TEntity).IsAssignableFrom(typeof(ISoftDelete));
    }

    public static bool IsTenantOwned { get; }
    public static bool IsSoftDelete { get; }

    protected EfCoreRepository(IServiceProvider serviceProvider)
    {
        UnitOfWorkManager = serviceProvider.GetRequiredService<IUnitOfWorkManager>();
        DbContextProvider = serviceProvider.GetRequiredService<IDbContextProvider<TDbContext>>();
        TenantContextAccessor = serviceProvider.GetRequiredService<ITenantContextAccessor>();
        FilterSwitch = serviceProvider.GetRequiredService<IFilterSwitch>();
        DbContextOptions = serviceProvider.GetRequiredService<AxiomDbContextOptions<TDbContext>>();
        EntityOptions = DbContextOptions.GetEntityOptions<TEntity>();

        var connectionStringProvider = serviceProvider.GetRequiredService<IConnectionStringProvider>();
        ConnectionStringContextOptions = connectionStringProvider.Contexts[DbContextOptions.ConnectionStringName];
    }

    public IUnitOfWork UnitOfWork => UnitOfWorkManager.RequiredCurrent;

    protected IUnitOfWorkManager UnitOfWorkManager { get; }
    protected IDbContextProvider<TDbContext> DbContextProvider { get; }
    protected ITenantContextAccessor TenantContextAccessor { get; }
    protected IFilterSwitch FilterSwitch { get; }
    protected AxiomDbContextOptions<TDbContext> DbContextOptions { get; }
    protected AxiomEntityOptions<TEntity> EntityOptions { get; }
    protected ConnectionStringContextOptions ConnectionStringContextOptions { get; }

    public virtual async Task<TEntity?> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        cancellationToken = GetCancellationToken(cancellationToken);
        var queryable = await GetQueryableAsync(includeDetails, cancellationToken);

        return await queryable.FirstOrDefaultAsync(
            predicate,
            cancellationToken: cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        cancellationToken = GetCancellationToken(cancellationToken);
        var queryable = await GetQueryableAsync(includeDetails, cancellationToken);

        if (predicate != null)
        {
            queryable = queryable.Where(predicate);
        }

        if (orderBy != null)
        {
            queryable = orderBy(queryable);
        }

        return await queryable.ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetPagedListAsync(
        int skip,
        int take,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy,
        Expression<Func<TEntity, bool>>? predicate = null,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        cancellationToken = GetCancellationToken(cancellationToken);
        var queryable = await GetQueryableAsync(includeDetails, cancellationToken);

        if (predicate != null)
        {
            queryable = queryable.Where(predicate);
        }

        queryable = orderBy(queryable);

        return await queryable
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<long> GetCountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken = GetCancellationToken(cancellationToken);
        var queryable = await GetQueryableAsync(false, cancellationToken);

        if (predicate == null)
        {
            return await queryable.LongCountAsync(cancellationToken);
        }

        return await queryable.LongCountAsync(predicate, cancellationToken);
    }

    public virtual async ValueTask<TEntity> AddAsync(
        TEntity entity,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        cancellationToken = GetCancellationToken(cancellationToken);
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
        cancellationToken = GetCancellationToken(cancellationToken);
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
        cancellationToken = GetCancellationToken(cancellationToken);
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
        cancellationToken = GetCancellationToken(cancellationToken);
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
        cancellationToken = GetCancellationToken(cancellationToken);
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
        cancellationToken = GetCancellationToken(cancellationToken);
        var set = await GetDbSetAsync(cancellationToken);

        set.RemoveRange(entities);

        if (autoSave)
        {
            await UnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    protected virtual ValueTask<TDbContext> GetDbContextAsync(CancellationToken cancellationToken = default)
    {
        if (!IsTenantOwned &&
            !ConnectionStringContextOptions.IsTenantAgnostic &&
            TenantContextAccessor.Current != null)
        {
            // This is a host-side (tenant-agnostic) DbSet being resolved while a tenant
            // is currently active. Temporarily clear the ambient tenant context so the
            // `ConnectionStringProvider` provides the host connection instead of the active tenant's.
            using (TenantContextAccessor.Change(current: null))
            {
                return DbContextProvider.GetAsync(cancellationToken);
            }
        }

        return DbContextProvider.GetAsync(cancellationToken);
    }

    protected virtual async ValueTask<DbSet<TEntity>> GetDbSetAsync(CancellationToken cancellationToken = default)
    {
        var context = await GetDbContextAsync(cancellationToken);
        return context.Set<TEntity>();
    }

    protected virtual async ValueTask<IQueryable<TEntity>> GetQueryableAsync(
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        var set = await GetDbSetAsync(cancellationToken);
        var queryable = set.AsQueryable();

        if (UnitOfWork.Options.TransactionBehavior == UnitOfWorkTransactionBehavior.Suppress)
        {
            queryable = queryable.AsNoTracking();
        }

        //queryable = queryable.IgnoreQueryFilters([nameof(ISoftDelete.IsDeleted)])
        
        return IncludeDetails(queryable, includeDetails);
    }

    protected virtual IQueryable<TEntity> IncludeDetails(IQueryable<TEntity> query, bool includeDetails = true)
    {
        if (!includeDetails)
        {
            return query;
        }

        return EntityOptions.IncludeDetails == null ? query : EntityOptions.IncludeDetails(query);
    }

    protected virtual CancellationToken GetCancellationToken(CancellationToken cancellationToken)
    {
        return cancellationToken.FallbackTo(UnitOfWork.CancellationToken);
    }
}

public class EfCoreRepository<TDbContext, TEntity, TKey>(
    IServiceProvider serviceProvider) :
    EfCoreRepository<TDbContext, TEntity>(serviceProvider),
    IRepository<TEntity, TKey>
    where TDbContext : DbContext
    where TEntity : class, IEntity<TKey>
    where TKey : notnull { }