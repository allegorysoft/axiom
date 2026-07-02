using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.Json;
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
    protected JsonNamingPolicy NamingPolicy { get; set; } = JsonNamingPolicy.KebabCaseLower;

    protected virtual FrozenDictionary<string, EventQueue> BuildQueues()
    {
        Options.Queue.Name ??= NamingPolicy.ConvertName(
            Assembly.GetEntryAssembly()?.GetName().Name
            ?? throw new InvalidOperationException("Event bus queue name cannot be null"));

        switch (Options.Queue.Topology)
        {
            case QueueTopology.Single:
                return BuildSingleQueue();
            case QueueTopology.PerMessageType:
                return BuildPerMessageTypeQueue();
            case QueueTopology.PerHandler:
                return BuildPerHandlerQueue();
            case QueueTopology.PerAssembly:
                return BuildPerAssemblyQueue();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected virtual FrozenDictionary<string, EventQueue> BuildSingleQueue()
    {
        var handlers = Options.Events.SelectMany(x => x.Handlers).Distinct().ToList();
        var events = new Dictionary<
            string,
            (DistributedEventDescriptor Descriptor, ImmutableArray<IEventHandler>.Builder Handlers)>();

        foreach (var handler in handlers)
        {
            var service = ServiceProvider.GetRequiredService(handler);

            var eventHandlerTypes = handler
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDistributedEventHandler<>));

            foreach (var eventHandlerType in eventHandlerTypes)
            {
                var eventType = eventHandlerType.GenericTypeArguments.Single();

                if (!events.TryGetValue(eventType.FullName!, out var eventItem))
                {
                    eventItem = new ValueTuple<DistributedEventDescriptor, ImmutableArray<IEventHandler>.Builder>(
                        Options.GetEvent(eventType),
                        ImmutableArray.CreateBuilder<IEventHandler>());
                    events[eventType.FullName!] = eventItem;
                }

                eventItem.Handlers.Add(CreateEventHandler(eventType, service));
            }
        }

        var queues = new Dictionary<string, EventQueue>(1)
        {
            [Options.Queue.Name!] = new(
                events.ToFrozenDictionary(
                    kvp => kvp.Key,
                    kvp => new EventRegistration(kvp.Value.Descriptor, kvp.Value.Handlers.ToImmutable())))
        };

        return queues.ToFrozenDictionary();
    }

    protected virtual FrozenDictionary<string, EventQueue> BuildPerMessageTypeQueue()
    {
        var queues = new Dictionary<string, EventQueue>(Options.Events.Length);

        foreach (var descriptor in Options.Events)
        {
            var queueName = string.Concat(Options.Queue.Name, ".", NamingPolicy.ConvertName(descriptor.Type.Name));
            var eventHandlers = ImmutableArray.CreateBuilder<IEventHandler>(descriptor.Handlers.Length);

            foreach (var handler in descriptor.Handlers)
            {
                var service = ServiceProvider.GetRequiredService(handler);
                eventHandlers.Add(CreateEventHandler(descriptor.Type, service));
            }

            var events = new Dictionary<string, EventRegistration>
            {
                [descriptor.Name] = new(descriptor, eventHandlers.ToImmutable())
            };

            queues[queueName] = new EventQueue(events.ToFrozenDictionary());
        }

        return queues.ToFrozenDictionary();
    }

    protected virtual FrozenDictionary<string, EventQueue> BuildPerHandlerQueue()
    {
        var handlers = Options.Events.SelectMany(x => x.Handlers).Distinct().ToList();
        var queues = new Dictionary<string, EventQueue>(handlers.Count);

        foreach (var handler in handlers)
        {
            var service = ServiceProvider.GetRequiredService(handler);
            var queueName = string.Concat(Options.Queue.Name, ".", NamingPolicy.ConvertName(handler.Name));
            var events = new Dictionary<
                string,
                (DistributedEventDescriptor Descriptor, ImmutableArray<IEventHandler>.Builder Handlers)>();

            var eventHandlerTypes = handler
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDistributedEventHandler<>));

            foreach (var eventHandlerType in eventHandlerTypes)
            {
                var eventType = eventHandlerType.GenericTypeArguments.Single();

                if (!events.TryGetValue(eventType.FullName!, out var eventItem))
                {
                    eventItem = new ValueTuple<DistributedEventDescriptor, ImmutableArray<IEventHandler>.Builder>(
                        Options.GetEvent(eventType),
                        ImmutableArray.CreateBuilder<IEventHandler>());
                    events[eventType.FullName!] = eventItem;
                }

                eventItem.Handlers.Add(CreateEventHandler(eventType, service));
            }

            queues[queueName] = new EventQueue(
                events.ToFrozenDictionary(
                    kvp => kvp.Key,
                    kvp => new EventRegistration(kvp.Value.Descriptor, kvp.Value.Handlers.ToImmutable())));
        }

        return queues.ToFrozenDictionary();
    }

    protected virtual FrozenDictionary<string, EventQueue> BuildPerAssemblyQueue()
    {
        var assemblies = Options.Events
            .SelectMany(x => x.Handlers)
            .Distinct()
            .GroupBy(g => g.Assembly, t => t)
            .ToList();
        var queues = new Dictionary<string, EventQueue>(assemblies.Count);

        foreach (var handlers in assemblies)
        {
            var queueName = string.Concat(Options.Queue.Name, ".", NamingPolicy.ConvertName(
                handlers.Key.GetName().Name ?? throw new InvalidOperationException("Assembly name cannot be null")));
            var events = new Dictionary<
                string,
                (DistributedEventDescriptor Descriptor, ImmutableArray<IEventHandler>.Builder Handlers)>();

            foreach (var handler in handlers)
            {
                var service = ServiceProvider.GetRequiredService(handler);
                var eventHandlerTypes = handler
                    .GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDistributedEventHandler<>));

                foreach (var eventHandlerType in eventHandlerTypes)
                {
                    var eventType = eventHandlerType.GenericTypeArguments.Single();

                    if (!events.TryGetValue(eventType.FullName!, out var eventItem))
                    {
                        eventItem = new ValueTuple<DistributedEventDescriptor, ImmutableArray<IEventHandler>.Builder>(
                            Options.GetEvent(eventType),
                            ImmutableArray.CreateBuilder<IEventHandler>());
                        events[eventType.FullName!] = eventItem;
                    }

                    eventItem.Handlers.Add(CreateEventHandler(eventType, service));
                }
            }

            queues[queueName] = new EventQueue(
                events.ToFrozenDictionary(
                    kvp => kvp.Key,
                    kvp => new EventRegistration(kvp.Value.Descriptor, kvp.Value.Handlers.ToImmutable())));
        }

        return queues.ToFrozenDictionary();
    }

    protected virtual IEventHandler CreateEventHandler(Type eventType, object service)
    {
        var handlerType = typeof(ServiceEventHandler<>).MakeGenericType(eventType);
        return (IEventHandler) Activator.CreateInstance(handlerType, service)!;
    }
}