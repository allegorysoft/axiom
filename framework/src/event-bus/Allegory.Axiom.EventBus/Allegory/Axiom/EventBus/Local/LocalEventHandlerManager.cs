using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        LazyHandlers = new Lazy<FrozenDictionary<Type, ImmutableArray<ILocalEventHandlerAdapter>>>(BuildHandlers);
    }

    public FrozenDictionary<Type, ImmutableArray<ILocalEventHandlerAdapter>> Handlers => LazyHandlers.Value;
    protected LocalEventBusOptions Options { get; }
    protected IServiceProvider ServiceProvider { get; }
    protected Lazy<FrozenDictionary<Type, ImmutableArray<ILocalEventHandlerAdapter>>> LazyHandlers { get; }

    protected virtual FrozenDictionary<Type, ImmutableArray<ILocalEventHandlerAdapter>> BuildHandlers()
    {
        var dictionary = new Dictionary<Type, ImmutableArray<ILocalEventHandlerAdapter>>(Options.Events.Count);

        foreach (var (eventType, handlerTypes) in Options.Events)
        {
            var serviceType = typeof(LocalEventHandlerAdapter<>).MakeGenericType(eventType);
            var handlers = ImmutableArray.CreateBuilder<ILocalEventHandlerAdapter>(handlerTypes.Length);

            foreach (var handler in handlerTypes)
            {
                var service = ServiceProvider.GetRequiredService(handler);
                handlers.Add((ILocalEventHandlerAdapter) Activator.CreateInstance(serviceType, service)!);
            }

            dictionary.Add(eventType, handlers.ToImmutable());
        }

        return dictionary.ToFrozenDictionary();
    }
}