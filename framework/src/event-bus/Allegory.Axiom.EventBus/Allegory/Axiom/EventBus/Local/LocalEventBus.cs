using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.UnitOfWork;

namespace Allegory.Axiom.EventBus.Local;

public class LocalEventBus(
    IUnitOfWorkManager unitOfWorkManager,
    LocalEventHandlerManager manager)
    : ILocalEventBus, ISingletonService
{
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected LocalEventHandlerManager Manager { get; } = manager;

    public virtual async Task PublishAsync<T>(
        T payload,
        LocalEventPublishMode publishMode = LocalEventPublishMode.OnUnitOfWorkComplete)
        where T : notnull
    {
        if (!Manager.Handlers.ContainsKey(typeof(T)))
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
        foreach (var handler in Manager.Handlers[typeof(T)])
        {
            await handler.HandleAsync(payload);
        }
    }
}