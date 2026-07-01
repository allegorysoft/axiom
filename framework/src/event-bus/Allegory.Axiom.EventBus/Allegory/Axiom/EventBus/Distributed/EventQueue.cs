using System.Collections.Generic;
using System.Collections.Immutable;

namespace Allegory.Axiom.EventBus.Distributed;

public class EventQueue
{
    // We wanna use Events, Handlers in one property?
    public Dictionary<string, List<IEventHandler>> Handlers { get; } = new();

    public ImmutableArray<DistributedEvent> Events { get; set; }
}