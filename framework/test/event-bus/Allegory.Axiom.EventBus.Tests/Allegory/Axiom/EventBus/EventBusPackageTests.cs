using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.EventBus.Distributed;
using Allegory.Axiom.EventBus.Local;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EventBus;

public class EventBusPackageTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected LocalEventBusOptions LocalOptions => fixture.Service<IOptions<LocalEventBusOptions>>().Value;
    protected DistributedEventBusOptions DistributedOptions => fixture.Service<IOptions<DistributedEventBusOptions>>().Value;

    [Fact]
    public void ShouldRegisterLocalEvents()
    {
        var eventItem = LocalOptions.Events.Single(x => x.Key == typeof(LocalTestEvent));
        eventItem.Value.ShouldBe([typeof(LocalTestEventHandler)]);

        var handler = fixture.Service<LocalTestEventHandler>();
        handler.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldRegisterDistributedEvents()
    {
        var eventItem = DistributedOptions.Events.Single(x => x.Type == typeof(DistributedTestEvent));
        eventItem.Name.ShouldBe(typeof(DistributedTestEvent).FullName);
        eventItem.Topic.ShouldBe("test.distributed-event-1");
        eventItem.Handlers.ShouldBe([typeof(DistributedTestEventHandler)]);

        var handler = fixture.Service<DistributedTestEventHandler>();
        handler.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldOrderHandlersByEventOrderAttribute()
    {
        LocalOptions.Events[typeof(OrderedTestEvent)].ShouldBe(
        [
            typeof(OrderedTestEventHandler1),
            typeof(OrderedTestEventHandler2),
            typeof(OrderedTestEventHandler3)
        ]);

        DistributedOptions.Events.Single(x => x.Type == typeof(OrderedTestEvent)).Handlers.ShouldBe(
        [
            typeof(DistributedOrderedTestEventHandler1),
            typeof(DistributedOrderedTestEventHandler2),
            typeof(DistributedOrderedTestEventHandler3)
        ]);
    }
}

file record LocalTestEvent {}

file class LocalTestEventHandler : ILocalEventHandler<LocalTestEvent>
{
    public Task HandleAsync(LocalTestEvent payload) => Task.CompletedTask;
}

[TopicName("test.distributed-event-1")]
file record DistributedTestEvent {}

file class DistributedTestEventHandler : IDistributedEventHandler<DistributedTestEvent>
{
    public Task HandleAsync(DistributedTestEvent payload) => Task.CompletedTask;
}

file record OrderedTestEvent {}

[EventOrder(2)]
file class OrderedTestEventHandler2 : ILocalEventHandler<OrderedTestEvent>
{
    public Task HandleAsync(OrderedTestEvent payload) => Task.CompletedTask;
}

[EventOrder(3)]
file class OrderedTestEventHandler3 : ILocalEventHandler<OrderedTestEvent>
{
    public Task HandleAsync(OrderedTestEvent payload) => Task.CompletedTask;
}

[EventOrder(1)]
file class OrderedTestEventHandler1 : ILocalEventHandler<OrderedTestEvent>
{
    public Task HandleAsync(OrderedTestEvent payload) => Task.CompletedTask;
}

[EventOrder(2)]
file class DistributedOrderedTestEventHandler2 : IDistributedEventHandler<OrderedTestEvent>
{
    public Task HandleAsync(OrderedTestEvent payload) => Task.CompletedTask;
}

[EventOrder(3)]
file class DistributedOrderedTestEventHandler3 : IDistributedEventHandler<OrderedTestEvent>
{
    public Task HandleAsync(OrderedTestEvent payload) => Task.CompletedTask;
}

[EventOrder(1)]
file class DistributedOrderedTestEventHandler1 : IDistributedEventHandler<OrderedTestEvent>
{
    public Task HandleAsync(OrderedTestEvent payload) => Task.CompletedTask;
}