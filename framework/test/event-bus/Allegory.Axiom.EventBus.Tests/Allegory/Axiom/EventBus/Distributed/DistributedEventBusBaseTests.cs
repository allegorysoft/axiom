using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.EventBus.Distributed.Inbox;
using Allegory.Axiom.EventBus.Distributed.Outbox;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EventBus.Distributed;

public class DistributedEventBusBaseTests : IntegrationTest
{
    // Test DistributedEventBusBase logic (publish modes, hooks, outbox routing) here.

    public IDistributedEventBus EventBus => Service<IDistributedEventBus>();

    protected override Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<IDistributedEventBus, DistributedEventBusImp>());
        builder.Services.AddSingleton<IInboxStore, InMemoryInboxStore>();
        builder.Services.AddSingleton<IOutboxStore, InMemoryOutboxStore>();

        builder.Services.Configure<DistributedEventBusOptions>(options =>
        {
            options.Inbox.UseFor = static _ => true;
            options.Outbox.UseFor = static _ => true;
        });

        return base.ConfigureAsync(builder);
    }

    [Fact]
    public async Task ShouldHookOutboxBeforeCompleteAndBrokerAfterComplete()
    {
        // Outbox mode  → BeforeComplete (persist to store before tx commits)
        // OnUnitOfWorkComplete → AfterComplete (publish to broker after tx commits)

        var handler = Service<TestEventHandler>();
        var uowManager = Service<IUnitOfWorkManager>();

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

[Dependency(AutoRegister = false)]
file class DistributedEventBusImp(
    ILogger<DistributedEventBusBase> logger,
    IOptions<DistributedEventBusOptions> options,
    DistributedEventHandlerManager eventHandlerManager,
    DistributedEventProcessor eventProcessor,
    IUnitOfWorkManager unitOfWorkManager,
    IInboxStore inboxStore,
    IOutboxStore outboxStore)
    : DistributedEventBusBase(logger, options, eventHandlerManager, eventProcessor, unitOfWorkManager, inboxStore, outboxStore)
{
    protected FrozenDictionary<Type, ImmutableArray<IDistributedEventHandlerAdapter>> Handlers { get; set; } = null!;

    protected override async Task PublishToOutboxAsync<T>(EventEnvelope<T> envelope)
    {
        foreach (var handler in Handlers[typeof(T)])
        {
            await handler.HandleAsync(envelope.Payload, new EventContext());
        }
    }

    protected override async Task PublishToMessageBrokerAsync<T>(EventEnvelope<T> envelope)
    {
        foreach (var handler in Handlers[typeof(T)])
        {
            await handler.HandleAsync(envelope.Payload, new EventContext());
        }
    }

    public override Task InitializeAsync()
    {
        var handlers = new Dictionary<Type, ImmutableArray<IDistributedEventHandlerAdapter>.Builder>();

        foreach (var queue in EventHandlerManager.Queues.Values)
        {
            foreach (var (_, eventEntry) in queue.Events)
            {
                if (!handlers.TryGetValue(eventEntry.Descriptor.Type, out var builder))
                {
                    builder = ImmutableArray.CreateBuilder<IDistributedEventHandlerAdapter>();
                    handlers[eventEntry.Descriptor.Type] = builder;
                }

                builder.AddRange(eventEntry.Handlers);
            }
        }

        Handlers = handlers.ToFrozenDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToImmutable());

        return Task.CompletedTask;
    }
}

[Dependency(AutoRegister = false)]
file class InMemoryOutboxStore : IOutboxStore {}

[Dependency(AutoRegister = false)]
file class InMemoryInboxStore : IInboxStore {}

file record TestEvent(int Value);

file class TestEventHandler : IDistributedEventHandler<TestEvent>
{
    public List<TestEvent> Received { get; } = [];

    public Task HandleAsync(TestEvent payload, EventContext context)
    {
        Received.Add(payload);
        return Task.CompletedTask;
    }
}