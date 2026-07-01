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

[TopicName("abc.event-1")]
public record Event1 {}

[TopicName("abc.event-2")]
public record Event2 {}

public class EventHandler1 : IDistributedEventHandler<Event1>
{
    public async Task HandleAsync(Event1 payload)
    {
        await Task.Delay(10_000);
    }
}

public class EventHandler2 : IDistributedEventHandler<Event1>, IDistributedEventHandler<Event2>
{
    public Guid Id { get; } = Guid.NewGuid();
    public async Task HandleAsync(Event1 payload)
    {
        await Task.Delay(10_000);
    }
    public Task HandleAsync(Event2 payload) => Task.CompletedTask;
}