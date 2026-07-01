using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        var queues = new Dictionary<string, EventQueue>(Options.Events.Length);
        var handlers = Options.Events.SelectMany(x => x.Handlers).Distinct().ToList();

        foreach (var handler in handlers)
        {
            var service = ServiceProvider.GetRequiredService(handler);
            var queueName = handler.FullName ?? throw new InvalidOperationException("Handler name cannot be null");
            var eventQueue = new Dictionary<
                string,// EventType.FullName
                (DistributedEventDescriptor Descriptor, ImmutableArray<IEventHandler>.Builder Handlers)>();

            var events = handler
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDistributedEventHandler<>));

            foreach (var type in events)
            {
                var eventType = type.GenericTypeArguments.Single();

                if (!eventQueue.TryGetValue(eventType.FullName!, out var eventItem))
                {
                    eventItem = new ValueTuple<DistributedEventDescriptor, ImmutableArray<IEventHandler>.Builder>(
                        Options.GetEvent(eventType),
                        ImmutableArray.CreateBuilder<IEventHandler>());
                    eventQueue[eventType.FullName!] = eventItem;
                }

                var eventHandler = typeof(ServiceEventHandler<>).MakeGenericType(eventType);
                eventItem.Handlers.Add((IEventHandler) Activator.CreateInstance(eventHandler, service)!);
            }

            queues[queueName] = new EventQueue(
                eventQueue.ToFrozenDictionary(
                    kvp => kvp.Key,
                    kvp => new EventRegistration(kvp.Value.Descriptor, kvp.Value.Handlers.ToImmutable())));
        }

        return queues.ToFrozenDictionary();
    }
}