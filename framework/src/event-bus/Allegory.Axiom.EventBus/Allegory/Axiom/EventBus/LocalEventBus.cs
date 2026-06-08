using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.UnitOfWork;

namespace Allegory.Axiom.EventBus;

public class LocalEventBus(
    IUnitOfWorkManager unitOfWorkManager,
    LocalEventHandlerFactory factory)
    : ILocalEventBus, ISingletonService
{
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected LocalEventHandlerFactory Factory { get; } = factory;

    public virtual async Task PublishAsync<T>(
        T payload,
        LocalMessagePublishMode publishMode = LocalMessagePublishMode.OnUnitOfWorkComplete)
        where T : notnull
    {
        if (!Factory.Handlers.ContainsKey(typeof(T)))
        {
            return;
        }

        if (publishMode == LocalMessagePublishMode.OnUnitOfWorkComplete && UnitOfWorkManager.Current != null)
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
        foreach (var handler in Factory.Handlers[typeof(T)])
        {
            await handler.HandleAsync(payload);
        }
    }
}