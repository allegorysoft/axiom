using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.EventBus.Local;

public class LocalEventHandlerManager : ISingletonService
{
    public LocalEventHandlerManager(
        IOptions<LocalEventBusOptions> options,
        IServiceProvider serviceProvider)
    {
        Options = options.Value;
        ServiceProvider = serviceProvider;
        LazyHandlers = new Lazy<FrozenDictionary<Type, ImmutableArray<IEventHandler>>>(BuildHandlers);
    }

    public FrozenDictionary<Type, ImmutableArray<IEventHandler>> Handlers => LazyHandlers.Value;
    protected LocalEventBusOptions Options { get; }
    protected IServiceProvider ServiceProvider { get; }
    protected Lazy<FrozenDictionary<Type, ImmutableArray<IEventHandler>>> LazyHandlers { get; }

    protected virtual FrozenDictionary<Type, ImmutableArray<IEventHandler>> BuildHandlers()
    {
        var dictionary = new Dictionary<Type, ImmutableArray<IEventHandler>>(Options.Events.Count);

        foreach (var (eventType, handlerTypes) in Options.Events)
        {
            var serviceType = typeof(ServiceEventHandler<>).MakeGenericType(eventType);
            var handlers = ImmutableArray.CreateBuilder<IEventHandler>(handlerTypes.Length);

            foreach (var handler in handlerTypes.OrderBy(EventOrderAttribute.Get))
            {
                var service = ServiceProvider.GetRequiredService(handler);
                handlers.Add((IEventHandler) Activator.CreateInstance(serviceType, service)!);
            }

            dictionary.Add(eventType, handlers.ToImmutable());
        }

        return dictionary.ToFrozenDictionary();
    }
}