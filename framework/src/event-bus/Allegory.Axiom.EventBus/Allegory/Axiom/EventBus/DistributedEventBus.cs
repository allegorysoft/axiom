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

    public virtual Task PublishAsync<T>(
        T payload,
        bool onUnitOfWorkComplete = true,
        bool useOutbox = true) where T : notnull
    {
        throw new System.NotImplementedException();
    }

    protected virtual Task PublishToBrokerAsync<T>(T payload)
    {
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