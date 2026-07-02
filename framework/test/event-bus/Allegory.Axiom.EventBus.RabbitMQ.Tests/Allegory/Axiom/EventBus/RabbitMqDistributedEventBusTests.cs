using System;
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
        var order = new Event1();

        //await EventBus.PublishAsync(order, DistributedEventPublishMode.Immediate);
    }
}

public record Event1 {}

public record Event2 {}

[EventOrder(2)]
public class EventHandler1 : IDistributedEventHandler<Event1>
{
    public async Task HandleAsync(Event1 payload)
    {
        await Task.Delay(60_000);
    }
}

[EventOrder(1)]
public class EventHandler2 : IDistributedEventHandler<Event2>//, IDistributedEventHandler<Event1>
{
    public Guid Id { get; } = Guid.NewGuid();

    public async Task HandleAsync(Event1 payload)
    {
        await Task.Delay(60_000);
    }

    public Task HandleAsync(Event2 payload) => Task.CompletedTask;
}