using System.Threading.Tasks;
using Xunit;

namespace Allegory.Axiom.EventBus;

public class RabbitMqDistributedEventBusTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected IDistributedEventBus EventBus => fixture.Service<IDistributedEventBus>();

    [Fact]
    public async Task Test()
    {
        var order = new OrderCreated()
        {
            Number = "001"
        };

        await Parallel.ForAsync(0, 10_000, async (_,_)=>
        {
            await EventBus.PublishAsync(order, DistributedMessagePublishMode.Immediate);
        });
    }
}

public class OrderCreated
{
    public string Number { get; set; }
}