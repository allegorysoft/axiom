namespace Allegory.Axiom.Domain.Entities;

public abstract class Entity : IEntity
{
    protected Entity()
    {
        // Set tenant if entity is ITenantOwned
    }
}

public abstract class Entity<TKey> : Entity, IEntity<TKey>
{
    public virtual TKey Id { get; protected set; } = default!;

    protected Entity(TKey id)
    {
        Id = id;
    }
}