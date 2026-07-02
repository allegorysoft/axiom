using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EventBus.Distributed;

public class DistributedEventHandlerManagerTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    [Fact]
    public void ShouldBuildSingleQueueByDefault()
    {
        var manager = fixture.Service<DistributedEventHandlerManager>();

        var queue = manager.Queues.Values.Single();

        queue.Events.ContainsKey(typeof(TestEvent).FullName!).ShouldBeTrue();
        queue.Events.ContainsKey(typeof(Event1).FullName!).ShouldBeTrue();
        queue.Events.ContainsKey(typeof(Event2).FullName!).ShouldBeTrue();

        queue.Events[typeof(TestEvent).FullName!].Handlers.Length.ShouldBe(2);
        queue.Events[typeof(Event1).FullName!].Handlers.Length.ShouldBe(2);
        queue.Events[typeof(Event2).FullName!].Handlers.Length.ShouldBe(1);
    }

    [Fact]
    public async Task ShouldBuildQueuePerEventTypeWhenTopologyIsPerEventType()
    {
        var provider = await fixture.CreateServiceProviderAsync(c =>
        {
            c.Services.Configure<DistributedEventBusOptions>(options =>
            {
                options.Queue.Name = "orders";
                options.Queue.Topology = QueueTopology.PerEventType;
            });
        });

        var manager = provider.GetRequiredService<DistributedEventHandlerManager>();
        var events = provider.GetRequiredService<IOptions<DistributedEventBusOptions>>().Value.Events;

        manager.Queues.Count.ShouldBe(events.Length);
        manager.Queues.Keys.ShouldAllBe(k => k.StartsWith("orders."));

        foreach (var queue in manager.Queues.Values)
        {
            queue.Events.Count.ShouldBe(1);
        }

        // Event1 has two handlers, so its queue must have both handlers within specified order
        var eventItem = manager.Queues.Values
            .Single(x => x.Events.ContainsKey(typeof(Event1).FullName!))
            .Events.Single().Value;
        eventItem.Descriptor.Handlers.ShouldBe([typeof(EventHandler2), typeof(EventHandler1)]);
    }

    [Fact]
    public async Task ShouldBuildQueuePerHandlerWhenTopologyIsPerHandler()
    {
        var provider = await fixture.CreateServiceProviderAsync(c =>
        {
            c.Services.Configure<DistributedEventBusOptions>(options =>
            {
                options.Queue.Name = "orders";
                options.Queue.Topology = QueueTopology.PerHandler;
            });
        });

        var manager = provider.GetRequiredService<DistributedEventHandlerManager>();
        var events = provider.GetRequiredService<IOptions<DistributedEventBusOptions>>().Value.Events;

        var distinctHandlerCount = events.SelectMany(e => e.Handlers).Distinct().Count();

        manager.Queues.Count.ShouldBe(distinctHandlerCount);
        manager.Queues.Keys.ShouldAllBe(k => k.StartsWith("orders."));

        // EventHandler2 handles both Event1 and Event2, so its queue must carry both.
        var eventQueue = manager.Queues.Values.Single(q => q.Events.ContainsKey(typeof(Event1).FullName!)
                                                           && q.Events.ContainsKey(typeof(Event2).FullName!));
        eventQueue.Events.Count.ShouldBe(2);
        foreach (var (_, eventItem) in eventQueue.Events)
        {
            eventItem.Descriptor.Handlers.ShouldContain(typeof(EventHandler2));
        }
    }

    [Fact]
    public async Task ShouldBuildQueuePerAssemblyWhenTopologyIsPerAssembly()
    {
        var provider = await fixture.CreateServiceProviderAsync(c =>
        {
            c.Services.Configure<DistributedEventBusOptions>(options =>
            {
                options.Queue.Name = "orders";
                options.Queue.Topology = QueueTopology.PerAssembly;
            });
        });

        var manager = provider.GetRequiredService<DistributedEventHandlerManager>();

        // All test handlers live in this same test assembly.
        manager.Queues.TryGetValue("orders.allegory.axiom.event-bus.tests", out var eventQueue).ShouldBeTrue();
        eventQueue.Events.ContainsKey(typeof(TestEvent).FullName!).ShouldBeTrue();
        eventQueue.Events.ContainsKey(typeof(Event1).FullName!).ShouldBeTrue();
        eventQueue.Events.ContainsKey(typeof(Event2).FullName!).ShouldBeTrue();
    }
}

file class Event1 {}

file class Event2 {}

file class EventHandler1 : IDistributedEventHandler<Event1>
{
    public Task HandleAsync(Event1 payload) => Task.CompletedTask;
}

[EventOrder(-1)]
file class EventHandler2 : IDistributedEventHandler<Event2>, IDistributedEventHandler<Event1>
{
    public Task HandleAsync(Event2 payload) => Task.CompletedTask;
    public Task HandleAsync(Event1 payload) => Task.CompletedTask;
}

file record TestEvent;

file class TestEventHandler1 : IDistributedEventHandler<TestEvent>
{
    public Task HandleAsync(TestEvent payload) => Task.CompletedTask;
}

file class TestEventHandler2 : IDistributedEventHandler<TestEvent>
{
    public Task HandleAsync(TestEvent payload) => Task.CompletedTask;
}