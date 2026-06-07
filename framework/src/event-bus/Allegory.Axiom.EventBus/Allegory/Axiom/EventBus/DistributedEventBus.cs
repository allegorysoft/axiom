using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.UnitOfWork;

namespace Allegory.Axiom.EventBus;

[Dependency(Strategy = RegistrationStrategy.TryAdd)]
public class DistributedEventBus(
    IUnitOfWorkManager unitOfWorkManager,
    DistributedEventHandlerFactory factory)
    : IDistributedEventBus, ISingletonService
{
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected DistributedEventHandlerFactory Factory { get; } = factory;

    public virtual async Task PublishAsync<T>(
        T payload,
        DispatchMode dispatchMode = DispatchMode.OnUnitOfWorkComplete,
        DeliveryMode deliveryMode = DeliveryMode.Outbox) where T : notnull
    {
        if (dispatchMode == DispatchMode.OnUnitOfWorkComplete && UnitOfWorkManager.Current != null)
        {
            UnitOfWorkManager.Current.AddHook(
                UnitOfWorkHookPoint.BeforeComplete,
                () => PublishToMessageBrokerAsync<T>(payload));
        }
        else
        {
            await PublishToMessageBrokerAsync<T>(payload);
        }
    }

    protected virtual Task PublishToMessageBrokerAsync<T>(T payload)
    {
        //Send to rabbitmq, kafka, etc.

        return Task.CompletedTask;
    }

    protected virtual async Task InvokeHandlersAsync<T>(object payload)
    {
        // We should create parent uow, all event handlers for an event should run inside same uow
        // We should create Activity, and use SetParent(traceparent) from coming event
        // Use "IntegrationEvent" suffix; `public record OrderCreatedIntegrationEvent(int OrderId);`

        foreach (var handler in Factory.Handlers[typeof(T)])
        {
            await handler.HandleAsync(payload);
        }
    }
}