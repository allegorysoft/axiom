using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.UnitOfWork;

namespace Allegory.Axiom.EventBus.Local;

public class LocalEventBus(
    IUnitOfWorkManager unitOfWorkManager,
    LocalEventHandlerManager eventHandlerManager)
    : ILocalEventBus, ISingletonService
{
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected LocalEventHandlerManager EventHandlerManager { get; } = eventHandlerManager;

    public virtual async Task PublishAsync<T>(
        T payload,
        LocalEventPublishMode publishMode = LocalEventPublishMode.OnUnitOfWorkComplete)
        where T : notnull
    {
        var payloadType = typeof(T) == typeof(object) ? payload.GetType() : typeof(T);

        if (!EventHandlerManager.Handlers.ContainsKey(payloadType))
        {
            return;
        }

        if (publishMode == LocalEventPublishMode.OnUnitOfWorkComplete && UnitOfWorkManager.Current != null)
        {
            if (UnitOfWorkManager.Current.Items.TryGetValue(ILocalEventBus.UnitOfWorkItemKey, out var obj))
            {
                var queue = (Queue<(object Payload, Type PayloadType)>) obj;
                queue.Enqueue((payload, payloadType));
            }
            else
            {
                InitializeUnitOfWorkEventQueue(payload, payloadType);
            }
        }
        else
        {
            await InvokeHandlersAsync(payload, payloadType);
        }
    }

    protected void InitializeUnitOfWorkEventQueue(object payload, Type payloadType)
    {
        var newQueue = new Queue<(object Payload, Type PayloadType)>();
        UnitOfWorkManager.Current!.Items[ILocalEventBus.UnitOfWorkItemKey] = newQueue;

        UnitOfWorkManager.Current.AddHook(
            UnitOfWorkHookPoint.BeforeComplete,
            async () =>
            {
                while (newQueue.TryDequeue(out var tuple))
                {
                    await InvokeHandlersAsync(tuple.Payload, tuple.PayloadType);
                }
            });

        newQueue.Enqueue((payload, payloadType));
    }

    protected virtual async Task InvokeHandlersAsync(object payload, Type type)
    {
        foreach (var handler in EventHandlerManager.Handlers[type])
        {
            await handler.HandleAsync(payload);
        }
    }
}