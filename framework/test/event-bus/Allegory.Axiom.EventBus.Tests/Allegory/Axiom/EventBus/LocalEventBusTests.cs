using System.Collections.Generic;
using System.Threading.Tasks;
using Allegory.Axiom.UnitOfWork;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EventBus;

public class LocalEventBusTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected ILocalEventBus EventBus => fixture.Service<ILocalEventBus>();

    [Fact]
    public async Task ShouldPublishEventToHandler()
    {
        var handler = fixture.Service<TestEventHandler>();

        handler.Received.ShouldNotContain(e => e.Value == 1);

        await EventBus.PublishAsync(new TestEvent(1), onUnitOfWorkComplete: false);

        handler.Received.ShouldContain(e => e.Value == 1);
    }

    [Fact]
    public async Task ShouldPublishEventToAllHandlers()
    {
        var handler1 = fixture.Service<TestEventHandler>();
        var handler2 = fixture.Service<TestEventHandler2>();

        handler1.Received.ShouldNotContain(e => e.Value == 2);
        handler2.Received.ShouldNotContain(e => e.Value == 2);

        await EventBus.PublishAsync(new TestEvent(2), onUnitOfWorkComplete: false);

        handler1.Received.ShouldContain(e => e.Value == 2);
        handler2.Received.ShouldContain(e => e.Value == 2);
    }

    [Fact]
    public async Task ShouldNotThrowWhenNoHandlerRegistered()
    {
        await Should.NotThrowAsync(() =>
            EventBus.PublishAsync(new UnhandledTestEvent(), onUnitOfWorkComplete: false));
    }

    [Fact]
    public async Task ShouldDeferHandlerUntilUnitOfWorkCompletes()
    {
        var handler = fixture.Service<TestEventHandler>();
        var uowManager = fixture.Service<IUnitOfWorkManager>();

        await using var uow = uowManager.Begin();
        await EventBus.PublishAsync(new TestEvent(3), onUnitOfWorkComplete: true);

        handler.Received.ShouldNotContain(e => e.Value == 3);

        await uow.CompleteAsync(TestContext.Current.CancellationToken);

        handler.Received.ShouldContain(e => e.Value == 3);
    }

    [Fact]
    public async Task ShouldInvokeHandlerImmediatelyWhenNoActiveUnitOfWork()
    {
        var handler = fixture.Service<TestEventHandler>();

        await EventBus.PublishAsync(new TestEvent(4), onUnitOfWorkComplete: true);

        handler.Received.ShouldContain(e => e.Value == 4);
    }

    [Fact]
    public async Task ShouldInvokeHandlerImmediatelyWhenOnUnitOfWorkCompleteIsFalse()
    {
        var handler = fixture.Service<TestEventHandler>();
        var uowManager = fixture.Service<IUnitOfWorkManager>();

        await using var uow = uowManager.Begin();
        await EventBus.PublishAsync(new TestEvent(5), onUnitOfWorkComplete: false);

        handler.Received.ShouldContain(e => e.Value == 5);

        await uow.CompleteAsync(TestContext.Current.CancellationToken);
    }
}

file record TestEvent(int Value);

file record UnhandledTestEvent;

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