using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using Allegory.Axiom.Extensibility;

namespace Allegory.Axiom.EventBus.Distributed;

public class DistributedEventBusOptions : IExtraProperties
{
    private FrozenDictionary<string, DistributedEventDescriptor> _namedEvents = null!;
    private FrozenDictionary<Type, DistributedEventDescriptor> _typedEvents = null!;

    public ImmutableArray<DistributedEventDescriptor> Events
    {
        get;
        set
        {
            if (value != default)
            {
                _namedEvents = value.ToFrozenDictionary(key => key.Name, value => value);
                _typedEvents = value.ToFrozenDictionary(key => key.Type, value => value);
            }

            field = value;
        }
    }
    public IDictionary<string, object?> ExtraProperties { get; } = new Dictionary<string, object?>();
    public QueueOptions Queue { get; set; } = new();
    public InboxOptions Inbox { get; set; } = new();
    public OutboxOptions Outbox { get; set; } = new();

    public DistributedEventDescriptor GetEvent(string name)
    {
        return _namedEvents[name];
    }

    public DistributedEventDescriptor GetEvent(Type type)
    {
        return _typedEvents[type];
    }

    public DistributedEventDescriptor GetEvent<T>()
    {
        return _typedEvents[typeof(T)];
    }
}