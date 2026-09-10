using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Domain.Entities;

namespace Allegory.Axiom.Domain.Repositories;

public interface IRepository { }

public interface IRepository<TEntity> :
    IReadOnlyRepository<TEntity>
    where TEntity : class, IEntity
{
    ValueTask<TEntity> AddAsync(
        TEntity entity,
        bool autoSave = false,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        bool autoSave = false,
        CancellationToken cancellationToken = default);

    ValueTask<TEntity> UpdateAsync(
        TEntity entity,
        bool autoSave = false,
        CancellationToken cancellationToken = default);

    Task UpdateRangeAsync(
        IEnumerable<TEntity> entities,
        bool autoSave = false,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        TEntity entity,
        bool autoSave = false,
        CancellationToken cancellationToken = default);

    Task RemoveRangeAsync(
        IEnumerable<TEntity> entities,
        bool autoSave = false,
        CancellationToken cancellationToken = default);
}

public interface IRepository<TEntity, TKey> :
    IRepository<TEntity>,
    IReadOnlyRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : notnull { }