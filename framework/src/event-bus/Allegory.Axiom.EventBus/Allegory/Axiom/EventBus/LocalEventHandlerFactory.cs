using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.EventBus;

public class LocalEventHandlerFactory(
    IOptions<LocalEventBusOptions> options,
    IServiceProvider serviceProvider)
    : ISingletonService
{
    protected LocalEventBusOptions Options { get; } = options.Value;
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    public virtual ConcurrentDictionary<string, List<IEventHandler>> GetHandlers()
    {
        var list = new ConcurrentDictionary<string, List<IEventHandler>>();

        foreach (var handlers in Options.Handlers)
        {
            var eventName = EventNameAttribute.Get(handlers.Key);
            var eventHandlers = new List<IEventHandler>();
            var handlerType = typeof(ServiceEventHandler<>).MakeGenericType(handlers.Key);
            
            foreach (var handler in handlers.Value.OrderBy(EventOrderAttribute.Get))
            {
                var service = ServiceProvider.GetRequiredService(handler);
                var eventHandler = (IEventHandler) ActivatorUtilities.CreateInstance(
                    ServiceProvider, handlerType, service);
                eventHandlers.Add(eventHandler);
            }

            if (!list.TryAdd(eventName, eventHandlers))
            {
                throw new InvalidOperationException($"Event {eventName} already exists");
            }
        }

        return list;
    }
}