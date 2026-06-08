using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.UnitOfWork;

namespace Allegory.Axiom.EventBus;

[Dependency(Strategy = RegistrationStrategy.TryAdd)]
public class DistributedEventBus(
    IUnitOfWorkManager unitOfWorkManager,
    DistributedEventHandlerFactory factory)
    : DistributedEventBusBase(unitOfWorkManager, factory)
{
    protected override Task PublishToMessageBrokerAsync<T>(T payload)
    {
        return Task.CompletedTask;
    }

    protected override Task PublishToOutboxAsync<T>(T payload)
    {
        // We shouldn't use outbox for default implementation
        return Task.CompletedTask;
    }
}