using System;
using System.Threading.Tasks;
using Allegory.Axiom.EventBus.Distributed;
using Microsoft.Extensions.Logging;

namespace Events;

public class EventHandler1(ILogger<EventHandler1> logger) : IDistributedEventHandler<Event1>
{
    protected ILogger<EventHandler1> Logger { get; } = logger;

    public virtual async Task HandleAsync(Event1 payload, EventContext context)
    {
        Logger.LogInformation("{EventHandler} - started: {Payload}", GetType().Name, payload);
        await Task.Delay(TimeSpan.FromSeconds(Random.Shared.Next(1, 5)), context.CancellationToken);
        Logger.LogInformation("{EventHandler} - completed: {Payload}", GetType().Name, payload);
    }
}