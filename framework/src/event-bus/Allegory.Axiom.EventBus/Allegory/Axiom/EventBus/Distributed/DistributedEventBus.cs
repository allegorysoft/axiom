using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.UnitOfWork;

namespace Allegory.Axiom.EventBus.Distributed;

[Dependency(Strategy = RegistrationStrategy.TryAdd)]
public class DistributedEventBus(
    IUnitOfWorkManager unitOfWorkManager,
    DistributedEventHandlerFactory factory)
    : DistributedEventBusBase(unitOfWorkManager, factory)
{
    public override Task PublishAsync<T>(
        T payload,
        DistributedMessagePublishMode publishMode = DistributedMessagePublishMode.Outbox)
    {
        return Factory.Handlers.ContainsKey(typeof(T))
            ? base.PublishAsync(payload, publishMode)
            : Task.CompletedTask;
    }

    protected override Task PublishToMessageBrokerAsync<T>(T payload) => InvokeHandlersAsync<T>(payload);

    protected override Task PublishToOutboxAsync<T>(T payload) => InvokeHandlersAsync<T>(payload);

    protected virtual async Task InvokeHandlersAsync<T>(object payload)
    {
        foreach (var handler in Factory.Handlers[typeof(T)])
        {
            await handler.HandleAsync(payload);
        }
    }

    public override Task InitializeAsync() => Task.CompletedTask;
}