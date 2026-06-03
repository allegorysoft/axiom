using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Disposables;
using Allegory.Axiom.UnitOfWork;

namespace Allegory.Axiom.EventBus;

public class LocalEventBus(
    IUnitOfWorkManager unitOfWorkManager,
    LocalEventHandlerFactory factory)
    : ILocalEventBus, ISingletonService
{
    protected ConcurrentDictionary<string, List<IEventHandler>> Handlers { get; } = factory.GetHandlers();
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;

    public virtual IDisposable Subscribe(string eventName, Func<object, Task> handler)
    {
        var eventHandler = new DelegateEventHandler(handler);

        var handlers = Handlers.GetOrAdd(eventName, _ => []);
        handlers.Add(eventHandler);

        return new DisposableDelegate(() => handlers.Remove(eventHandler));
    }

    public IDisposable Subscribe<T>(Func<T, Task> handler) where T : notnull
    {
        var eventHandler = new DelegateEventHandler<T>(handler);
        var eventName = EventNameAttribute.Get<T>();

        var handlers = Handlers.GetOrAdd(eventName, _ => []);
        handlers.Add(eventHandler);

        return new DisposableDelegate(() => handlers.Remove(eventHandler));
    }

    public virtual async Task PublishAsync(string eventName, object payload, bool onUnitOfWorkComplete = true)
    {
        if (onUnitOfWorkComplete && UnitOfWorkManager.Current != null)
        {
            UnitOfWorkManager.Current.AddHook(
                UnitOfWorkHookPoint.BeforeComplete,
                async () => await InvokeHandlersAsync(eventName, payload));
        }
        else
        {
            await InvokeHandlersAsync(eventName, payload);
        }
    }

    public virtual async Task PublishAsync<T>(T payload, bool onUnitOfWorkComplete = true) where T : notnull
    {
        var eventName = EventNameAttribute.Get<T>();
        await PublishAsync(eventName, payload, onUnitOfWorkComplete);
    }

    protected virtual async Task InvokeHandlersAsync(string eventName, object payload)
    {
        if (!Handlers.TryGetValue(eventName, out var handlers))
        {
            return;
        }

        foreach (var handler in handlers)
        {
            await handler.HandleAsync(payload);
        }
    }
}