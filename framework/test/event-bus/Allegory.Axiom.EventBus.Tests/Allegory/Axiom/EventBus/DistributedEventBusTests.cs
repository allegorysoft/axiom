using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Allegory.Axiom.EventBus.Distributed;
using Allegory.Axiom.UnitOfWork;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EventBus;

public class DistributedEventBusTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected IDistributedEventBus EventBus => fixture.Service<IDistributedEventBus>();

    [Fact]
    public async Task ShouldPublishEventToHandler()
    {
        var handler = fixture.Service<TestEventHandler>();

        handler.Received.ShouldNotContain(e => e.Value == 1);

        await EventBus.PublishAsync(new TestEvent(1));

        handler.Received.ShouldContain(e => e.Value == 1);
    }

    [Fact]
    public async Task ShouldPublishValueTypeEventToHandler()
    {
        var handler = fixture.Service<ValueTestEventHandler>();

        handler.Received.ShouldNotContain(e => e.Value == 1);

        await EventBus.PublishAsync(new ValueTestEvent(1));

        handler.Received.ShouldContain(e => e.Value == 1);
    }

    [Fact]
    public async Task ShouldPublishEventToAllHandlers()
    {
        var handler1 = fixture.Service<TestEventHandler>();
        var handler2 = fixture.Service<TestEventHandler2>();

        handler1.Received.ShouldNotContain(e => e.Value == 2);
        handler2.Received.ShouldNotContain(e => e.Value == 2);

        await EventBus.PublishAsync(new TestEvent(2));

        handler1.Received.ShouldContain(e => e.Value == 2);
        handler2.Received.ShouldContain(e => e.Value == 2);
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
    public async Task ShouldInvokeHandlerImmediatelyWhenPublishModeIsImmediate()
    {
        var handler = fixture.Service<TestEventHandler>();
        var uowManager = fixture.Service<IUnitOfWorkManager>();

        await using var uow = uowManager.Begin();
        await EventBus.PublishAsync(new TestEvent(3), publishMode: DistributedEventPublishMode.Immediate);

        handler.Received.ShouldContain(e => e.Value == 3);

        await uow.CompleteAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ShouldInvokeHandlerImmediatelyWhenNoActiveUnitOfWork()
    {
        var handler = fixture.Service<TestEventHandler>();

        await EventBus.PublishAsync(
            new TestEvent(4),
            publishMode: DistributedEventPublishMode.OnUnitOfWorkComplete);
        await EventBus.PublishAsync(
            new TestEvent(5),
            publishMode: DistributedEventPublishMode.Outbox);

        handler.Received.ShouldContain(e => e.Value == 4);
        handler.Received.ShouldContain(e => e.Value == 5);
    }

    [Fact]
    public async Task ShouldDeferHandlerUntilUnitOfWorkCompletes()
    {
        var handler = fixture.Service<TestEventHandler>();
        var uowManager = fixture.Service<IUnitOfWorkManager>();

        await using var uow = uowManager.Begin();

        await EventBus.PublishAsync(
            new TestEvent(6),
            publishMode: DistributedEventPublishMode.OnUnitOfWorkComplete);
        await EventBus.PublishAsync(
            new TestEvent(7),
            publishMode: DistributedEventPublishMode.Outbox);

        handler.Received.ShouldNotContain(e => e.Value == 6);
        handler.Received.ShouldNotContain(e => e.Value == 7);

        await uow.CompleteAsync(TestContext.Current.CancellationToken);

        handler.Received.ShouldContain(e => e.Value == 6);
        handler.Received.ShouldContain(e => e.Value == 7);
    }

    [Fact]
    public async Task ShouldHookOutboxBeforeCompleteAndBrokerAfterComplete()
    {
        // Outbox mode  → BeforeComplete (persist to store before tx commits)
        // OnUnitOfWorkComplete → AfterComplete (publish to broker after tx commits)

        var handler = fixture.Service<TestEventHandler>();
        var uowManager = fixture.Service<IUnitOfWorkManager>();

        await using var uow = uowManager.Begin();

        await EventBus.PublishAsync(
            new TestEvent(8),
            publishMode: DistributedEventPublishMode.OnUnitOfWorkComplete);
        await EventBus.PublishAsync(
            new TestEvent(9),
            publishMode: DistributedEventPublishMode.Outbox);

        handler.Received.ShouldNotContain(e => e.Value == 8);
        handler.Received.ShouldNotContain(e => e.Value == 9);

        uow.AddHook(UnitOfWorkHookPoint.BeforeComplete, () =>
        {
            // Outbox already saved; broker publish not yet fired
            handler.Received.ShouldContain(e => e.Value == 9);
            handler.Received.ShouldNotContain(e => e.Value == 8);
            return Task.CompletedTask;
        });

        uow.AddHook(UnitOfWorkHookPoint.AfterComplete, () =>
        {
            // Broker publish fired; both handled
            handler.Received.ShouldContain(e => e.Value == 8);
            return Task.CompletedTask;
        });

        await uow.CompleteAsync(TestContext.Current.CancellationToken);
    }
}

file record TestEvent(int Value);

file record UnhandledTestEvent;

file record ThrowingTestEvent;

file record struct ValueTestEvent(int Value);

file class TestEventHandler : IDistributedEventHandler<TestEvent>
{
    public List<TestEvent> Received { get; } = [];

    public Task HandleAsync(TestEvent payload)
    {
        Received.Add(payload);
        return Task.CompletedTask;
    }
}

file class TestEventHandler2 : IDistributedEventHandler<TestEvent>
{
    public List<TestEvent> Received { get; } = [];

    public Task HandleAsync(TestEvent payload)
    {
        Received.Add(payload);
        return Task.CompletedTask;
    }
}

file class ValueTestEventHandler : IDistributedEventHandler<ValueTestEvent>
{
    public List<ValueTestEvent> Received { get; } = [];

    public Task HandleAsync(ValueTestEvent payload)
    {
        Received.Add(payload);
        return Task.CompletedTask;
    }
}

file class ThrowingTestEventHandler : IDistributedEventHandler<ThrowingTestEvent>
{
    public Task HandleAsync(ThrowingTestEvent payload) =>
        throw new InvalidOperationException("handler-failure");
}