using System.Collections.Generic;

namespace Allegory.Axiom.Domain.Entities;

public abstract class AggregateRoot : Entity, IAggregateRoot
{
    private List<object>? _localEvents;
    private List<object>? _distributedEvents;

    public virtual IReadOnlyList<object> GetLocalEvents() => _localEvents ?? (IReadOnlyList<object>) [];
    public virtual void ClearLocalEvents() => _localEvents?.Clear();

    protected virtual void AddLocalEvent(object payload)
    {
        _localEvents ??= [];
        _localEvents.Add(payload);
    }

    public virtual IReadOnlyList<object> GetDistributedEvents() => _distributedEvents ?? (IReadOnlyList<object>) [];
    public virtual void ClearDistributedEvents() => _distributedEvents?.Clear();

    protected virtual void AddDistributedEvent(object payload)
    {
        _distributedEvents ??= [];
        _distributedEvents.Add(payload);
    }
}

public abstract class AggregateRoot<TKey> : AggregateRoot, IAggregateRoot<TKey> where TKey : notnull
{
    public virtual TKey Id { get; protected set; } = default!;

    protected AggregateRoot() { }

    public override object[] GetKeys() => [Id];
}