using System.Collections.Frozen;

namespace Allegory.Axiom.EventBus.Distributed;

public class EventQueue(FrozenDictionary<string, EventRegistration> events)
{
    public FrozenDictionary<string, EventRegistration> Events { get; } = events;
}