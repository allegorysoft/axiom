using System;
using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus;

public interface IDistributedEventBus
{
    // We should create parent uow, all event handlers for an event should run inside same uow
    // We should create Activity, and use SetParent(traceparent) from coming event
    // Use "IntegrationEvent" suffix; `public record OrderCreatedIntegrationEvent(int OrderId);`

    Task PublishAsync<TEvent>(TEvent payload, bool onUnitOfWorkComplete = true, bool useOutbox = true);
    IDisposable Subscribe<TEvent>(Func<TEvent, Task> onEvent);
}