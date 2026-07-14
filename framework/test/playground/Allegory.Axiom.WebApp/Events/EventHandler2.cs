using System;
using System.Threading.Tasks;
using Allegory.Axiom.EventBus.Distributed;
using Microsoft.Extensions.Logging;

namespace Events;

public class EventHandler2(
    ILogger<EventHandler1> logger,
    IDistributedEventBus distributedEventBus)
    : IDistributedEventHandler<Event1>, IDistributedEventHandler<Event2>
{
    protected ILogger<EventHandler1> Logger { get; } = logger;
    protected IDistributedEventBus DistributedEventBus { get; } = distributedEventBus;

    public virtual async Task HandleAsync(Event1 payload, EventContext context)
    {
        Logger.LogInformation("{EventHandler} - started: {Payload}", GetType().Name, payload);
        await Task.Delay(TimeSpan.FromSeconds(Random.Shared.Next(1, 5)), context.CancellationToken);
        await DistributedEventBus.PublishAsync(new Event2(payload.Number));
        Logger.LogInformation("{EventHandler} - completed: {Payload}", GetType().Name, payload);
    }

    public virtual async Task HandleAsync(Event2 payload, EventContext context)
    {
        Logger.LogInformation("{EventHandler} - started (child): {Payload}", GetType().Name, payload);
        await Task.Delay(TimeSpan.FromSeconds(Random.Shared.Next(1, 5)), context.CancellationToken);
        Logger.LogInformation("{EventHandler} - completed (child): {Payload}", GetType().Name, payload);
    }
}