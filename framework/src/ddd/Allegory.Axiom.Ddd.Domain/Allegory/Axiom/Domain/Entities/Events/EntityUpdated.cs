namespace Allegory.Axiom.Domain.Entities.Events;

public sealed class EntityUpdated<TEntity>(TEntity entity, TEntity previous) where TEntity : IEntity
{
    public TEntity Entity { get; } = entity;
    public TEntity Previous { get; } = previous;
}