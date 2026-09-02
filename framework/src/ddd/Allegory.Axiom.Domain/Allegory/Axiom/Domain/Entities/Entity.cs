namespace Allegory.Axiom.Domain.Entities;

public abstract class Entity : IEntity
{
    protected Entity()
    {
        EntityAccessor.TrySetTenant(this);
    }

    public abstract object[] GetKeys();
}

public abstract class Entity<TKey> : Entity, IEntity<TKey> where TKey : notnull
{
    public TKey Id { get; protected set; } = default!;

    protected Entity() { }

    protected Entity(TKey id)
    {
        Id = id;
    }

    public override object[] GetKeys() => [Id];
}
