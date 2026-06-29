using System.Threading.Tasks;
using Allegory.Axiom.EventBus.Distributed;
using Xunit;

namespace Allegory.Axiom.EventBus;

public class RabbitMqDistributedEventBusTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected IDistributedEventBus EventBus => fixture.Service<IDistributedEventBus>();

    [Fact]
    public async Task Test()
    {
        var order = new OrderCreated
        {
            Number = "001"
        };

        await EventBus.PublishAsync(order, DistributedEventPublishMode.Immediate);
    }
}

public class OrderCreated
{
    public string Number { get; set; }
}