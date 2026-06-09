using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.EventBus;

public class DistributedEventHandlerFactory : ISingletonService
{
    public DistributedEventHandlerFactory(
        IOptions<DistributedEventBusOptions> options,
        IServiceProvider serviceProvider)
    {
        Options = options.Value;
        ServiceProvider = serviceProvider;
        LazyHandlers = new Lazy<FrozenDictionary<Type, ImmutableArray<IEventHandler>>>(GetHandlers);
    }

    public FrozenDictionary<Type, ImmutableArray<IEventHandler>> Handlers => LazyHandlers.Value;
    protected DistributedEventBusOptions Options { get; }
    protected IServiceProvider ServiceProvider { get; }
    protected Lazy<FrozenDictionary<Type, ImmutableArray<IEventHandler>>> LazyHandlers { get; }

    protected virtual FrozenDictionary<Type, ImmutableArray<IEventHandler>> GetHandlers()
    {
        var dictionary = new Dictionary<Type, ImmutableArray<IEventHandler>>(Options.Handlers.Count);

        foreach (var (eventType, handlerTypes) in Options.Handlers)
        {
            var serviceType = typeof(ServiceEventHandler<>).MakeGenericType(eventType);
            var handlers = ImmutableArray.CreateBuilder<IEventHandler>(handlerTypes.Length);

            //We can't use `OrderBy(EventOrderAttribute.Get)` in distributed events
            //Each handler (for same event type) independent of each other
            foreach (var handler in handlerTypes)
            {
                var service = ServiceProvider.GetRequiredService(handler);
                handlers.Add((IEventHandler) Activator.CreateInstance(serviceType, service)!);
            }

            dictionary.Add(eventType, handlers.ToImmutable());
        }

        return dictionary.ToFrozenDictionary();
    }
}