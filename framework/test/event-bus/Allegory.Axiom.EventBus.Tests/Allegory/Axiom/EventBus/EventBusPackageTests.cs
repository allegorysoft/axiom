using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.EventBus.Distributed;
using Allegory.Axiom.EventBus.Local;
using Allegory.Axiom.Priority;
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
        var eventEntry = LocalOptions.Events.Single(x => x.Key == typeof(LocalTestEvent));
        eventEntry.Value.ShouldBe([typeof(LocalTestEventHandler)]);

        var handler = fixture.Service<LocalTestEventHandler>();
        handler.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldRegisterDistributedEvents()
    {
        var eventEntry = DistributedOptions.Events.Single(x => x.Type == typeof(DistributedTestEvent));
        eventEntry.Name.ShouldBe(typeof(DistributedTestEvent).FullName);
        eventEntry.Topic.ShouldBe("test.distributed-event-1");
        eventEntry.Handlers.ShouldBe([typeof(DistributedTestEventHandler)]);

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
    public Task HandleAsync(DistributedTestEvent payload, EventContext context) => Task.CompletedTask;
}

file record OrderedTestEvent {}

[Priority(PriorityLevel.Normal)]
file class OrderedTestEventHandler2 : ILocalEventHandler<OrderedTestEvent>
{
    public Task HandleAsync(OrderedTestEvent payload) => Task.CompletedTask;
}

[Priority(PriorityLevel.Low)]
file class OrderedTestEventHandler3 : ILocalEventHandler<OrderedTestEvent>
{
    public Task HandleAsync(OrderedTestEvent payload) => Task.CompletedTask;
}

[Priority(PriorityLevel.High)]
file class OrderedTestEventHandler1 : ILocalEventHandler<OrderedTestEvent>
{
    public Task HandleAsync(OrderedTestEvent payload) => Task.CompletedTask;
}

[Priority(PriorityLevel.Normal)]
file class DistributedOrderedTestEventHandler2 : IDistributedEventHandler<OrderedTestEvent>
{
    public Task HandleAsync(OrderedTestEvent payload, EventContext context) => Task.CompletedTask;
}

[Priority(PriorityLevel.Low)]
file class DistributedOrderedTestEventHandler3 : IDistributedEventHandler<OrderedTestEvent>
{
    public Task HandleAsync(OrderedTestEvent payload, EventContext context) => Task.CompletedTask;
}

[Priority(PriorityLevel.High)]
file class DistributedOrderedTestEventHandler1 : IDistributedEventHandler<OrderedTestEvent>
{
    public Task HandleAsync(OrderedTestEvent payload, EventContext context) => Task.CompletedTask;
}