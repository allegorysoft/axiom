using Allegory.Axiom.Domain.Entities;

namespace Allegory.Axiom.Domain.Repositories;

public interface IRepository { }

public interface IRepository<TEntity> :
    IReadOnlyRepository<TEntity>
    where TEntity : class, IEntity
{
    // Add
    // Update
    // Delete
}

public interface IRepository<TEntity, TKey> :
    IRepository<TEntity>,
    IReadOnlyRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
}