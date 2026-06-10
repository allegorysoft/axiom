using System.Threading.Tasks;
using Allegory.Axiom.RabbitMQ;
using Allegory.Axiom.UnitOfWork;

namespace Allegory.Axiom.EventBus;

public class RabbitMqDistributedEventBus(
    RabbitMqClientFactory clientFactory,
    IUnitOfWorkManager unitOfWorkManager,
    DistributedEventHandlerFactory handlerFactory)
    : DistributedEventBusBase(unitOfWorkManager, handlerFactory)
{
    public RabbitMqClientFactory ClientFactory { get; } = clientFactory;

    protected override Task PublishToMessageBrokerAsync<T>(T payload)
    {

        throw new System.NotImplementedException();
    }

    public virtual async Task InitializeAsync()
    {
        var client = await ClientFactory.GetAsync("event-bus");
        var publisherChannel = await client.GetChannelAsync("publisher");
    }
}