using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus.Distributed;

public class EventQueue
{
    public HashSet<string> Topics { get; } = [];
    public Dictionary<string, List<IEventHandler>> Handlers { get; } = new();

    //PrefetchCount, etc.
}

[EventName("abc.event-1")]
public record Event1 {}

public record Event2 {}

public class EventHandler1 : IDistributedEventHandler<Event1>
{
    public Task HandleAsync(Event1 payload) => Task.CompletedTask;
}

public class EventHandler2 : IDistributedEventHandler<Event1>, IDistributedEventHandler<Event2>
{
    public Guid Id { get; } = Guid.NewGuid();
    public Task HandleAsync(Event1 payload) => Task.CompletedTask;
    public Task HandleAsync(Event2 payload) => Task.CompletedTask;
}