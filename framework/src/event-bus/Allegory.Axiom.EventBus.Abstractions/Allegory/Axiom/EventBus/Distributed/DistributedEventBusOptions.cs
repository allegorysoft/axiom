using System;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Allegory.Axiom.EventBus.Distributed;

public class DistributedEventBusOptions
{
    private FrozenDictionary<string, DistributedEvent> _namedEvents = null!;
    private FrozenDictionary<Type, DistributedEvent> _typedEvents = null!;

    public required ImmutableArray<DistributedEvent> Events
    {
        get;
        set
        {
            _namedEvents = value.ToFrozenDictionary(key => key.Name, value => value);
            _typedEvents = value.ToFrozenDictionary(key => key.Type, value => value);
            field = value;
        }
    }
    public InboxOptions Inbox { get; set; } = new();
    public OutboxOptions Outbox { get; set; } = new();

    public DistributedEvent GetEvent(string name)
    {
        return _namedEvents[name];
    }

    public DistributedEvent GetEvent(Type type)
    {
        return _typedEvents[type];
    }

    public DistributedEvent GetEvent<T>()
    {
        return _typedEvents[typeof(T)];
    }
}