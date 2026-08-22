using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Priority;
using Allegory.Axiom.UnitOfWork;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EventBus.Local;

public class LocalEventBusTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected ILocalEventBus EventBus => fixture.Service<ILocalEventBus>();
    protected int Number { get; } = Random.Shared.Next(); // Each test method gets new number

    [Fact]
    public async Task ShouldPublishEventToHandler()
    {
        var handler = fixture.Service<TestEventHandler>();

        handler.Received.ShouldNotContain(e => e.Value == Number);

        await EventBus.PublishAsync(new TestEvent(Number));

        handler.Received.ShouldContain(e => e.Value == Number);
    }

    [Fact]
    public async Task ShouldPublishValueTypeEventToHandler()
    {
        var handler = fixture.Service<ValueTestEventHandler>();

        handler.Received.ShouldNotContain(e => e.Value == Number);

        await EventBus.PublishAsync(new ValueTestEvent(Number));

        handler.Received.ShouldContain(e => e.Value == Number);
    }

    [Fact]
    public async Task ShouldPublishGenericEventToHandler()
    {
        var handler = fixture.Service<GenericTestEventHandler>();
        var handler2 = fixture.Service<GenericTestEventHandler2>();

        var event1 = new GenericTestEvent<int>(Number);
        await EventBus.PublishAsync(event1);

        var event2 = new GenericTestEvent<string>(Number.ToString());
        await EventBus.PublishAsync(event2);

        handler.Received.ShouldContain(event1);
        handler.Received.Count.ShouldBe(1);
        handler2.Received.ShouldContain(event2);
        handler2.Received.Count.ShouldBe(1);
    }

    [Fact]
    public async Task ShouldPublishEventToHandlerWhenPayloadTypeIsObject()
    {
        var handler = fixture.Service<TestEventHandler>();

        handler.Received.ShouldNotContain(e => e.Value == Number);

        object payload = new TestEvent(Number);
        await EventBus.PublishAsync(payload);

        handler.Received.ShouldContain(e => e.Value == Number);
    }

    [Fact]
    public async Task ShouldPublishEventToAllHandlers()
    {
        var handler1 = fixture.Service<TestEventHandler>();
        var handler2 = fixture.Service<TestEventHandler2>();

        handler1.Received.ShouldNotContain(e => e.Value == Number);
        handler2.Received.ShouldNotContain(e => e.Value == Number);

        await EventBus.PublishAsync(new TestEvent(Number));

        handler1.Received.ShouldContain(e => e.Value == Number);
        handler2.Received.ShouldContain(e => e.Value == Number);
    }

    [Fact]
    public async Task ShouldNotThrowWhenNoHandlerRegistered()
    {
        await Should.NotThrowAsync(() =>
            EventBus.PublishAsync(new UnhandledTestEvent()));
    }

    [Fact]
    public async Task ShouldExposeExceptionFromHandler()
    {
        await Should.ThrowAsync<InvalidOperationException>(() =>
            EventBus.PublishAsync(new ThrowingTestEvent()));
    }

    [Fact]
    public async Task ShouldDeferHandlerUntilUnitOfWorkCompletes()
    {
        var handler = fixture.Service<TestEventHandler>();
        var uowManager = fixture.Service<IUnitOfWorkManager>();

        await using var uow = uowManager.Begin(cancellationToken: TestContext.Current.CancellationToken);
        await EventBus.PublishAsync(new TestEvent(Number), publishMode: LocalEventPublishMode.OnUnitOfWorkComplete);

        handler.Received.ShouldNotContain(e => e.Value == Number);

        await uow.CompleteAsync(CancellationToken.None);

        handler.Received.ShouldContain(e => e.Value == Number);
    }

    [Fact]
    public async Task ShouldInvokeHandlerImmediatelyWhenNoActiveUnitOfWork()
    {
        var handler = fixture.Service<TestEventHandler>();

        await EventBus.PublishAsync(new TestEvent(Number), publishMode: LocalEventPublishMode.OnUnitOfWorkComplete);

        handler.Received.ShouldContain(e => e.Value == Number);
    }

    [Fact]
    public async Task ShouldInvokeHandlerImmediatelyWhenPublishModeIsImmediate()
    {
        var handler = fixture.Service<TestEventHandler>();
        var uowManager = fixture.Service<IUnitOfWorkManager>();

        await using var uow = uowManager.Begin(cancellationToken: TestContext.Current.CancellationToken);
        await EventBus.PublishAsync(new TestEvent(Number), publishMode: LocalEventPublishMode.Immediate);

        handler.Received.ShouldContain(e => e.Value == Number);

        await uow.CompleteAsync(CancellationToken.None);
    }

    [Fact]
    public async Task ShouldInvokeHandlersInSpecifiedOrder()
    {
        var payload = new OrderedTestEvent();
        await EventBus.PublishAsync(payload);

        payload.Items.ShouldBe(
        [
            typeof(OrderTestEventHandler1),
            typeof(OrderTestEventHandler2),
            typeof(OrderTestEventHandler3)
        ]);
    }
}

file record TestEvent(int Value);

file record GenericTestEvent<T>(T Value);

file record UnhandledTestEvent;

file record ThrowingTestEvent;

file record OrderedTestEvent
{
    public List<Type> Items { get; } = [];
}

file record struct ValueTestEvent(int Value);

file class TestEventHandler : ILocalEventHandler<TestEvent>
{
    public List<TestEvent> Received { get; } = [];

    public Task HandleAsync(TestEvent payload)
    {
        Received.Add(payload);
        return Task.CompletedTask;
    }
}

file class TestEventHandler2 : ILocalEventHandler<TestEvent>
{
    public List<TestEvent> Received { get; } = [];

    public Task HandleAsync(TestEvent payload)
    {
        Received.Add(payload);
        return Task.CompletedTask;
    }
}

file class GenericTestEventHandler : ILocalEventHandler<GenericTestEvent<int>>
{
    public List<GenericTestEvent<int>> Received { get; } = [];

    public Task HandleAsync(GenericTestEvent<int> payload)
    {
        Received.Add(payload);
        return Task.CompletedTask;
    }
}

file class GenericTestEventHandler2 : ILocalEventHandler<GenericTestEvent<string>>
{
    public List<GenericTestEvent<string>> Received { get; } = [];

    public Task HandleAsync(GenericTestEvent<string> payload)
    {
        Received.Add(payload);
        return Task.CompletedTask;
    }
}

file class ValueTestEventHandler : ILocalEventHandler<ValueTestEvent>
{
    public List<ValueTestEvent> Received { get; } = [];

    public Task HandleAsync(ValueTestEvent payload)
    {
        Received.Add(payload);
        return Task.CompletedTask;
    }
}

file class ThrowingTestEventHandler : ILocalEventHandler<ThrowingTestEvent>
{
    public Task HandleAsync(ThrowingTestEvent payload) =>
        throw new InvalidOperationException("handler-failure");
}

[Priority(PriorityLevel.Low)]
file class OrderTestEventHandler3 : ILocalEventHandler<OrderedTestEvent>
{
    public Task HandleAsync(OrderedTestEvent payload)
    {
        payload.Items.Add(typeof(OrderTestEventHandler3));
        return Task.CompletedTask;
    }
}

[Priority(PriorityLevel.Normal)]
file class OrderTestEventHandler2 : ILocalEventHandler<OrderedTestEvent>
{
    public Task HandleAsync(OrderedTestEvent payload)
    {
        payload.Items.Add(typeof(OrderTestEventHandler2));
        return Task.CompletedTask;
    }
}

[Priority(PriorityLevel.High)]
file class OrderTestEventHandler1 : ILocalEventHandler<OrderedTestEvent>
{
    public Task HandleAsync(OrderedTestEvent payload)
    {
        payload.Items.Add(typeof(OrderTestEventHandler1));
        return Task.CompletedTask;
    }
}