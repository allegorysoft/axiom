using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.UnitOfWork;

namespace Allegory.Axiom.Domain.Repositories;

public interface IReadOnlyRepository<TEntity> : IRepository where TEntity : class, IEntity
{
    IUnitOfWork UnitOfWork { get; }

    Task<TEntity?> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool includeDetails = true,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool includeDetails = false,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> GetPagedListAsync(
        int skip,
        int take,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy,
        Expression<Func<TEntity, bool>>? predicate = null,
        bool includeDetails = false,
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);
}

public interface IReadOnlyRepository<TEntity, TKey> : IReadOnlyRepository<TEntity>
    where TEntity : class, IEntity<TKey>
    where TKey : notnull
{
    Task<TEntity?> FindAsync(
        TKey id,
        bool includeDetails = true,
        CancellationToken cancellationToken = default);
}