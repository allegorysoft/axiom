using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.EventBus.Distributed.Inbox;
using Allegory.Axiom.EventBus.Distributed.Outbox;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.EventBus.Distributed;

[Dependency(Strategy = RegistrationStrategy.TryAdd)]
public class DistributedEventBus(
    IUnitOfWorkManager unitOfWorkManager,
    DistributedEventHandlerFactory handlerFactory,
    IOptions<DistributedEventBusOptions> options,
    IInboxStore inboxStore,
    IOutboxStore outboxStore)
    : DistributedEventBusBase(unitOfWorkManager, handlerFactory, options, inboxStore, outboxStore)
{
    public override Task PublishAsync<T>(
        T payload,
        DistributedEventPublishMode publishMode = DistributedEventPublishMode.Auto)
    {
        return HandlerFactory.Handlers.ContainsKey(typeof(T))
            ? base.PublishAsync(payload, publishMode)
            : Task.CompletedTask;
    }

    protected override Task PublishToMessageBrokerAsync<T>(T payload) => InvokeHandlersAsync<T>(payload);

    protected override Task PublishToOutboxAsync<T>(T payload) => InvokeHandlersAsync<T>(payload);

    protected virtual async Task InvokeHandlersAsync<T>(object payload)
    {
        foreach (var handler in HandlerFactory.Handlers[typeof(T)])
        {
            await handler.HandleAsync(payload);
        }
    }

    public override Task InitializeAsync() => Task.CompletedTask;
}