using System.Collections.Generic;

namespace Allegory.Axiom.EventBus.Distributed;

public class EventQueue
{
    public HashSet<string> Topics { get; } = [];
    public Dictionary<string, List<IEventHandler>> Handlers { get; } = new();
}