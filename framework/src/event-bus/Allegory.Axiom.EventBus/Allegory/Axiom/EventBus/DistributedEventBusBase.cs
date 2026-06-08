using System;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.UnitOfWork;

namespace Allegory.Axiom.EventBus;

public abstract class DistributedEventBusBase(
    IUnitOfWorkManager unitOfWorkManager,
    DistributedEventHandlerFactory factory)
    : IDistributedEventBus, ISingletonService
{
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected DistributedEventHandlerFactory Factory { get; } = factory;

    public virtual async Task PublishAsync<T>(
        T payload,
        DistributedMessagePublishMode publishMode = DistributedMessagePublishMode.Outbox)
        where T : notnull
    {
        if (publishMode == DistributedMessagePublishMode.Immediate)
        {
            await PublishToMessageBrokerAsync(payload);
            return;
        }

        if (UnitOfWorkManager.Current == null)
        {
            if (publishMode == DistributedMessagePublishMode.OnUnitOfWorkComplete)
            {
                await PublishToMessageBrokerAsync(payload);
            }
            else
            {
                await PublishToOutboxAsync(payload);
            }

            return;
        }

        if (publishMode == DistributedMessagePublishMode.OnUnitOfWorkComplete)
        {
            UnitOfWorkManager.Current.AddHook(
                UnitOfWorkHookPoint.AfterComplete,
                () => PublishToMessageBrokerAsync(payload));
        }
        else
        {
            UnitOfWorkManager.Current.AddHook(
                UnitOfWorkHookPoint.BeforeComplete,
                () => PublishToOutboxAsync(payload));
        }
    }

    protected virtual Task PublishToOutboxAsync<T>(T payload)
    {
        //Save to store

        return Task.CompletedTask;
    }

    //Send to rabbitmq, kafka, etc.
    protected abstract Task PublishToMessageBrokerAsync<T>(T payload);

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

public class DistributedEventContext<T>
{
    //TraceId?
    public required T Payload { get; init; }
}