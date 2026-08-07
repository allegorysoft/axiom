using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EventBus.Distributed;

public class InProcessDistributedEventBusTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected IDistributedEventBus EventBus => fixture.Service<IDistributedEventBus>();
    protected int Number { get; } = Random.Shared.Next();

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
    public async Task ShouldInvokeHandlerImmediatelyWhenPublishModeIsImmediate()
    {
        var handler = fixture.Service<TestEventHandler>();
        var uowManager = fixture.Service<IUnitOfWorkManager>();

        await using var uow = uowManager.Begin(cancellationToken: TestContext.Current.CancellationToken);
        await EventBus.PublishAsync(new TestEvent(Number), publishMode: DistributedEventPublishMode.Immediate);

        handler.Received.ShouldContain(e => e.Value == Number);

        await uow.CompleteAsync(CancellationToken.None);
    }

    [Fact]
    public async Task ShouldInvokeHandlerImmediatelyWhenNoActiveUnitOfWork()
    {
        var handler = fixture.Service<TestEventHandler>();
        var event1 = Random.Shared.Next();
        var event2 = Random.Shared.Next();
        var event3 = Random.Shared.Next();

        await EventBus.PublishAsync(
            new TestEvent(event1),
            publishMode: DistributedEventPublishMode.OnUnitOfWorkComplete);
        await EventBus.PublishAsync(
            new TestEvent(event2),
            publishMode: DistributedEventPublishMode.Outbox);
        await EventBus.PublishAsync(
            new TestEvent(event3),
            publishMode: DistributedEventPublishMode.Auto);

        handler.Received.ShouldContain(e => e.Value == event1);
        handler.Received.ShouldContain(e => e.Value == event2);
        handler.Received.ShouldContain(e => e.Value == event3);
    }

    [Fact]
    public async Task ShouldInvokeHandlerOnUnitOfWorkHookBeforeCompleteWhenPublishModeIsNotImmediateAndActiveUnitOfWork()
    {
        // The in-process event bus does not support the Outbox pattern.
        // Outbox mode is treated as OnUnitOfWorkComplete.
        // OnUnitOfWorkComplete -> BeforeComplete
        // Outbox               -> BeforeComplete
        // Auto                 -> BeforeComplete

        var handler = fixture.Service<TestEventHandler>();
        var uowManager = fixture.Service<IUnitOfWorkManager>();
        var event1 = Random.Shared.Next();
        var event2 = Random.Shared.Next();
        var event3 = Random.Shared.Next();

        await using var uow = uowManager.Begin(cancellationToken: TestContext.Current.CancellationToken);

        await EventBus.PublishAsync(
            new TestEvent(event1),
            publishMode: DistributedEventPublishMode.OnUnitOfWorkComplete);
        await EventBus.PublishAsync(
            new TestEvent(event2),
            publishMode: DistributedEventPublishMode.Outbox);
        await EventBus.PublishAsync(
            new TestEvent(event3),
            publishMode: DistributedEventPublishMode.Auto);

        handler.Received.ShouldNotContain(e => e.Value == event1);
        handler.Received.ShouldNotContain(e => e.Value == event2);
        handler.Received.ShouldNotContain(e => e.Value == event3);

        uow.AddHook(UnitOfWorkHookPoint.BeforeComplete, () =>
        {
            handler.Received.ShouldContain(e => e.Value == event1);
            handler.Received.ShouldContain(e => e.Value == event2);
            handler.Received.ShouldContain(e => e.Value == event3);
            return Task.CompletedTask;
        });

        await uow.CompleteAsync(CancellationToken.None);
    }

    [Fact]
    public async Task ShouldShareScopedServiceAcrossHandlersForAnEvent()
    {
        var uowManager = fixture.Service<IUnitOfWorkManager>();

        var handler1 = fixture.Service<ScopedEventHandler1>();
        var handler2 = fixture.Service<ScopedEventHandler2>();

        handler1.Received.ShouldBeNull();
        handler2.Received.ShouldBeNull();

        await using var uow = uowManager.Begin(cancellationToken: TestContext.Current.CancellationToken);

        await EventBus.PublishAsync(new ScopedEvent(), DistributedEventPublishMode.Immediate);

        handler1.Received.ShouldNotBeNull();
        handler2.Received.ShouldNotBeNull();
        handler1.Received.Id.ShouldBe(handler2.Received.Id);
        handler1.Received.ShouldBeSameAs(handler2.Received);
    }
}

file record TestEvent(int Value);

file record UnhandledTestEvent;

file record ThrowingTestEvent;

file record struct ValueTestEvent(int Value);

file class TestEventHandler : IDistributedEventHandler<TestEvent>
{
    public List<TestEvent> Received { get; } = [];

    public Task HandleAsync(TestEvent payload, EventContext context)
    {
        Received.Add(payload);
        return Task.CompletedTask;
    }
}

file class TestEventHandler2 : IDistributedEventHandler<TestEvent>
{
    public List<TestEvent> Received { get; } = [];

    public Task HandleAsync(TestEvent payload, EventContext context)
    {
        Received.Add(payload);
        return Task.CompletedTask;
    }
}

file class ValueTestEventHandler : IDistributedEventHandler<ValueTestEvent>
{
    public List<ValueTestEvent> Received { get; } = [];

    public Task HandleAsync(ValueTestEvent payload, EventContext context)
    {
        Received.Add(payload);
        return Task.CompletedTask;
    }
}

file class ThrowingTestEventHandler : IDistributedEventHandler<ThrowingTestEvent>
{
    public Task HandleAsync(ThrowingTestEvent payload, EventContext context) =>
        throw new InvalidOperationException("handler-failure");
}

file class ScopedImplementation : IScopedService
{
    public Guid Id { get; } = Guid.NewGuid();
}

file record ScopedEvent {}

file class ScopedEventHandler1(IUnitOfWorkManager manager) : IDistributedEventHandler<ScopedEvent>
{
    public ScopedImplementation? Received { get; protected set; }

    public Task HandleAsync(ScopedEvent payload, EventContext context)
    {
        Received = manager.RequiredCurrent.ServiceProvider.GetRequiredService<ScopedImplementation>();
        return Task.CompletedTask;
    }
}

file class ScopedEventHandler2(IUnitOfWorkManager manager) : IDistributedEventHandler<ScopedEvent>
{
    public ScopedImplementation? Received { get; protected set; }

    public Task HandleAsync(ScopedEvent payload, EventContext context)
    {
        Received = manager.RequiredCurrent.ServiceProvider.GetRequiredService<ScopedImplementation>();
        return Task.CompletedTask;
    }
}