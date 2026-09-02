namespace Allegory.Axiom.Domain.Entities.Events;

public sealed class EntityCreated<TEntity>(TEntity entity) where TEntity : IEntity
{
    public TEntity Entity { get; } = entity;
}