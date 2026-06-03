using System;
using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus;

/// <summary>
/// Distributed section
/// We should create parent uow, all event handlers for an event should run inside same uow
/// We should create Activity, and use SetParent(traceparent) from coming event
/// </summary>

public interface IDistributedEventBus
{
    Task PublishAsync<TEvent>(TEvent payload, bool onUnitOfWorkComplete = true, bool useOutbox = true);
    IDisposable Subscribe<TEvent>(Func<TEvent, Task> onEvent);
    //Unsubscribe
}

public interface IDistributedEventHandler<in T>
{
    Task HandleAsync(T payload);
}

public record OrderCreatedIntegrationEvent(int OrderId);

// IDistributedEventBus.PublishAsync(OrderCreatedIntegrationEvent)

public class DistributedOrderCreatedHandler : IDistributedEventHandler<OrderCreatedIntegrationEvent>
{
    public Task HandleAsync(OrderCreatedIntegrationEvent payload)
    {
        throw new NotImplementedException();
    }
}