using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.EventBus.Distributed;

public class DistributedEventHandlerManager : ISingletonService
{
    public DistributedEventHandlerManager(
        IOptions<DistributedEventBusOptions> options,
        IServiceProvider serviceProvider)
    {
        Options = options.Value;
        ServiceProvider = serviceProvider;
        LazyQueues = new Lazy<FrozenDictionary<string, EventQueue>>(BuildQueues);
    }

    public FrozenDictionary<string, EventQueue> Queues => LazyQueues.Value;
    protected DistributedEventBusOptions Options { get; }
    protected IServiceProvider ServiceProvider { get; }
    protected Lazy<FrozenDictionary<string, EventQueue>> LazyQueues { get; }

    protected virtual FrozenDictionary<string, EventQueue> BuildQueues()
    {
        // Create event queues based on option (Single, ForEachHandler, ForEachNamespace, etc.)
        var queues = new Dictionary<string, EventQueue>(Options.Events.Length);
        var handlers = Options.Events.SelectMany(x => x.Handlers).Distinct().ToList();

        foreach (var handler in handlers)
        {
            var queue = new EventQueue();
            var service = ServiceProvider.GetRequiredService(handler);

            var events = handler
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDistributedEventHandler<>));

            foreach (var type in events)
            {
                var eventType = type.GenericTypeArguments.Single();
                queue.Topics.Add(EventNameAttribute.Get(eventType));

                if (!queue.Handlers.TryGetValue(eventType.FullName!, out var eventHandlers))
                {
                    eventHandlers = [];
                    queue.Handlers[eventType.FullName!] = eventHandlers;
                }

                var eventHandler = typeof(ServiceEventHandler<>).MakeGenericType(eventType);
                eventHandlers.Add((IEventHandler) Activator.CreateInstance(eventHandler, service)!);
            }

            queues[handler.FullName!] = queue;
        }

        return queues.ToFrozenDictionary();
    }
}

