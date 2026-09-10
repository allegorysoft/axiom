namespace Allegory.Axiom.Domain.Entities.Events;

public sealed class EntityChanged<TEntity>(
    TEntity entity,
    EntityChangeType changeType)
    where TEntity : IEntity
{
    public TEntity Entity { get; } = entity;
    public EntityChangeType ChangeType { get; } = changeType;
}