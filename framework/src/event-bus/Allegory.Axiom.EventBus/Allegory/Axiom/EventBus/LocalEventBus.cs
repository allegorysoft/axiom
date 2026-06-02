using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Disposables;
using Allegory.Axiom.UnitOfWork;

namespace Allegory.Axiom.EventBus;

public class LocalEventBus(IUnitOfWorkManager unitOfWorkManager) : ILocalEventBus, ISingletonService
{
    protected ConcurrentDictionary<string, List<Func<object, Task>>> Handlers { get; } = [];
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;

    public virtual IDisposable Subscribe(string eventName, Func<object, Task> handler)
    {
        var handlers = Handlers.GetOrAdd(eventName, _ => []);
        handlers.Add(handler);

        return new DisposableDelegate(() => handlers.Remove(handler));
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

    public virtual void Unsubscribe(string eventName, Func<object, Task> handler)
    {
        var handlers = Handlers.GetOrAdd(eventName, _ => []);
        handlers.Remove(handler);
    }

    protected virtual async Task InvokeHandlersAsync(string eventName, object payload)
    {
        var handlers = Handlers.GetOrAdd(eventName, _ => []);
        foreach (var handler in handlers)
        {
            await handler.Invoke(payload);
        }
    }
}