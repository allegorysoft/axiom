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
        if (!EventHandlerManager.Handlers.ContainsKey(typeof(T)))
        {
            return;
        }

        if (publishMode == LocalEventPublishMode.OnUnitOfWorkComplete && UnitOfWorkManager.Current != null)
        {
            UnitOfWorkManager.Current.AddHook(
                UnitOfWorkHookPoint.BeforeComplete,
                () => InvokeHandlersAsync<T>(payload));
        }
        else
        {
            await InvokeHandlersAsync<T>(payload);
        }
    }

    protected virtual async Task InvokeHandlersAsync<T>(object payload)
    {
        foreach (var handler in EventHandlerManager.Handlers[typeof(T)])
        {
            await handler.HandleAsync(payload);
        }
    }
}