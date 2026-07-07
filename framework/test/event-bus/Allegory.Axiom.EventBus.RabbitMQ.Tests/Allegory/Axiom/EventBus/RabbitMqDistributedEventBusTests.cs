using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.EventBus.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EventBus;

public class RabbitMqDistributedEventBusTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected IDistributedEventBus EventBus => fixture.Service<IDistributedEventBus>();

    private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(15);
        var deadline = DateTime.UtcNow + timeout;

        while (!condition())
        {
            if (DateTime.UtcNow > deadline)
            {
                throw new TimeoutException("Condition was not met within the allotted time.");
            }

            await Task.Delay(50);
        }
    }

    [Fact]
    public async Task ShouldPublishAndConsumeEventThroughBroker()
    {
        var handler = fixture.Service<RabbitTestEventHandler>();

        await EventBus.PublishAsync(new RabbitTestEvent(1));

        await WaitUntilAsync(() => handler.Received.Contains(1));

        handler.Received.ShouldContain(1);
    }

    [Fact]
    public async Task ShouldDeliverEventToAllRegisteredHandlers()
    {
        var handler1 = fixture.Service<RabbitMultiTestEventHandler1>();
        var handler2 = fixture.Service<RabbitMultiTestEventHandler2>();

        await EventBus.PublishAsync(new RabbitMultiTestEvent(2));

        await WaitUntilAsync(
            () => handler1.Received.Contains(2) && handler2.Received.Contains(2),
            TimeSpan.FromSeconds(15));

        handler1.Received.ShouldContain(2);
        handler2.Received.ShouldContain(2);
    }

    [Fact]
    public async Task ShouldPublishValueTypeEventThroughBroker()
    {
        var handler = fixture.Service<RabbitValueTestEventHandler>();

        await EventBus.PublishAsync(new RabbitValueTestEvent(3));

        await WaitUntilAsync(() => handler.Received.Contains(3));

        handler.Received.ShouldContain(3);
    }

    [Fact]
    public async Task ShouldShareScopedServiceAcrossHandlersForSameEvent()
    {
        var handler1 = fixture.Service<RabbitScopedTestEventHandler1>();
        var handler2 = fixture.Service<RabbitScopedTestEventHandler2>();

        await EventBus.PublishAsync(new RabbitScopedTestEvent());

        await WaitUntilAsync(
            () => handler1.Received != null && handler2.Received != null,
            TimeSpan.FromSeconds(15));

        handler1.Received.ShouldBeSameAs(handler2.Received);
        handler1.Received!.Id.ShouldBe(handler2.Received!.Id);
    }

    [Fact]
    public async Task ShouldInvokeHandlersInOrderSpecifiedByEventOrderAttribute()
    {
        var handler3 = fixture.Service<RabbitOrderedTestEventHandler3>();

        await EventBus.PublishAsync(new RabbitOrderedTestEvent());

        await WaitUntilAsync(() => handler3.CapturedOrder != null);

        handler3.CapturedOrder.ShouldBe(
        [
            nameof(RabbitOrderedTestEventHandler1),
            nameof(RabbitOrderedTestEventHandler2),
            nameof(RabbitOrderedTestEventHandler3)
        ]);
    }
}

file record RabbitTestEvent(int Value);

file class RabbitTestEventHandler : IDistributedEventHandler<RabbitTestEvent>
{
    public ConcurrentBag<int> Received { get; } = [];

    public Task HandleAsync(RabbitTestEvent payload, EventContext context)
    {
        Received.Add(payload.Value);
        return Task.CompletedTask;
    }
}

file record RabbitMultiTestEvent(int Value);

file class RabbitMultiTestEventHandler1 : IDistributedEventHandler<RabbitMultiTestEvent>
{
    public ConcurrentBag<int> Received { get; } = [];

    public Task HandleAsync(RabbitMultiTestEvent payload, EventContext context)
    {
        Received.Add(payload.Value);
        return Task.CompletedTask;
    }
}

file class RabbitMultiTestEventHandler2 : IDistributedEventHandler<RabbitMultiTestEvent>
{
    public ConcurrentBag<int> Received { get; } = [];

    public Task HandleAsync(RabbitMultiTestEvent payload, EventContext context)
    {
        Received.Add(payload.Value);
        return Task.CompletedTask;
    }
}

file record struct RabbitValueTestEvent(int Value);

file class RabbitValueTestEventHandler : IDistributedEventHandler<RabbitValueTestEvent>
{
    public ConcurrentBag<int> Received { get; } = [];

    public Task HandleAsync(RabbitValueTestEvent payload, EventContext context)
    {
        Received.Add(payload.Value);
        return Task.CompletedTask;
    }
}

file class RabbitScopedImplementation : IScopedService
{
    public Guid Id { get; } = Guid.NewGuid();
}

file record RabbitScopedTestEvent;

file class RabbitScopedTestEventHandler1 : IDistributedEventHandler<RabbitScopedTestEvent>
{
    public RabbitScopedImplementation? Received { get; private set; }

    public Task HandleAsync(RabbitScopedTestEvent payload, EventContext context)
    {
        Received = context.ServiceProvider.GetRequiredService<RabbitScopedImplementation>();
        return Task.CompletedTask;
    }
}

file class RabbitScopedTestEventHandler2 : IDistributedEventHandler<RabbitScopedTestEvent>
{
    public RabbitScopedImplementation? Received { get; private set; }

    public Task HandleAsync(RabbitScopedTestEvent payload, EventContext context)
    {
        Received = context.ServiceProvider.GetRequiredService<RabbitScopedImplementation>();
        return Task.CompletedTask;
    }
}

file record RabbitOrderedTestEvent
{
    public List<string> Items { get; init; } = [];
}

[EventOrder(1)]
file class RabbitOrderedTestEventHandler1 : IDistributedEventHandler<RabbitOrderedTestEvent>
{
    public Task HandleAsync(RabbitOrderedTestEvent payload, EventContext context)
    {
        payload.Items.Add(nameof(RabbitOrderedTestEventHandler1));
        return Task.CompletedTask;
    }
}

[EventOrder(3)]
file class RabbitOrderedTestEventHandler3 : IDistributedEventHandler<RabbitOrderedTestEvent>
{
    public List<string>? CapturedOrder { get; private set; }

    public Task HandleAsync(RabbitOrderedTestEvent payload, EventContext context)
    {
        payload.Items.Add(nameof(RabbitOrderedTestEventHandler3));
        CapturedOrder = [..payload.Items];
        return Task.CompletedTask;
    }
}

[EventOrder(2)]
file class RabbitOrderedTestEventHandler2 : IDistributedEventHandler<RabbitOrderedTestEvent>
{
    public Task HandleAsync(RabbitOrderedTestEvent payload, EventContext context)
    {
        payload.Items.Add(nameof(RabbitOrderedTestEventHandler2));
        return Task.CompletedTask;
    }
}