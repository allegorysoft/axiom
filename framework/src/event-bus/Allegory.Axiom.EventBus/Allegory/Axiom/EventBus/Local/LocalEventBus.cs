using System;
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
            UnitOfWorkManager.Current.AddHook(
                UnitOfWorkHookPoint.BeforeComplete,
                () => InvokeHandlersAsync(payload, payloadType));
        }
        else
        {
            await InvokeHandlersAsync(payload, payloadType);
        }

    }

    protected virtual async Task InvokeHandlersAsync(object payload, Type type)
    {
        foreach (var handler in EventHandlerManager.Handlers[type])
        {
            await handler.HandleAsync(payload);
        }
    }
}