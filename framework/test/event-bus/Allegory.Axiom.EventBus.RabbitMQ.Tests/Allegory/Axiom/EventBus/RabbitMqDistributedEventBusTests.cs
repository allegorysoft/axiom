using System;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.EventBus.Distributed;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Allegory.Axiom.EventBus;

public class RabbitMqDistributedEventBusTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected IDistributedEventBus EventBus => fixture.Service<IDistributedEventBus>();

    [Fact]
    public async Task Test()
    {
        var order = new Event1();

        //await EventBus.PublishAsync(order, DistributedEventPublishMode.Immediate);
    }
}

public record Event1 {}

public class EventHandler1 : IDistributedEventHandler<Event1>
{
    public async Task HandleAsync(Event1 payload)
    {
        await Task.Delay(10_000);
    }
}
