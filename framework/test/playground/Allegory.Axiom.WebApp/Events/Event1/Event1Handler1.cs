using System;
using System.Threading.Tasks;
using Allegory.Axiom.EventBus.Distributed;
using Microsoft.Extensions.Logging;

namespace Events.Event1;

public class Event1Handler1(ILogger<Event1Handler1> logger) : IDistributedEventHandler<Event1>
{
    protected ILogger<Event1Handler1> Logger { get; } = logger;

    public virtual async Task HandleAsync(Event1 payload, EventContext context)
    {
        Logger.LogInformation("{Event} - started: {Payload}", GetType().Name, payload);
        await Task.Delay(TimeSpan.FromSeconds(5), context.CancellationToken);
        Logger.LogInformation("{Event} - completed: {Payload}", GetType().Name, payload);
    }
}