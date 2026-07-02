using System.Collections.Frozen;

namespace Allegory.Axiom.EventBus.Distributed;

public class EventQueue(FrozenDictionary<string, EventQueueEntry> events)
{
    public FrozenDictionary<string, EventQueueEntry> Events { get; } = events;
}