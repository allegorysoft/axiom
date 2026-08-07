using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.EventBus.Distributed.Inbox;
using Allegory.Axiom.EventBus.Distributed.Outbox;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EventBus.Distributed;

public class DistributedEventBusBaseTests(
    DistributedEventBusBaseFixture fixture)
    : IClassFixture<DistributedEventBusBaseFixture>
{
    // Test DistributedEventBusBase logic (publish modes, hooks, outbox routing) here.

    public IDistributedEventBus EventBus => fixture.Service<IDistributedEventBus>();
    protected int Number { get; } = Random.Shared.Next(); // Each test method gets new number

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
    public async Task ShouldPublishImmediatelyWhenPublishModeIsImmediate()
    {
        var handler = fixture.Service<TestEventHandler>();
        var uowManager = fixture.Service<IUnitOfWorkManager>();

        await using var uow = uowManager.Begin(cancellationToken: TestContext.Current.CancellationToken);
        await EventBus.PublishAsync(new TestEvent(Number), publishMode: DistributedEventPublishMode.Immediate);

        // Immediate skips unit of work entirely, no hook wait needed
        handler.Received.ShouldContain(e => e.Value == Number);

        await uow.CompleteAsync(CancellationToken.None);
    }

    [Fact]
    public async Task ShouldPublishImmediatelyWhenNoActiveUnitOfWork()
    {
        var handler = fixture.Service<TestEventHandler>();
        var number1 = Random.Shared.Next();
        var number2 = Random.Shared.Next();
        var number3 = Random.Shared.Next();

        await EventBus.PublishAsync(
            new TestEvent(number1),
            publishMode: DistributedEventPublishMode.OnUnitOfWorkComplete);
        await EventBus.PublishAsync(
            new TestEvent(number2),
            publishMode: DistributedEventPublishMode.Outbox);
        await EventBus.PublishAsync(
            new TestEvent(number3),
            publishMode: DistributedEventPublishMode.Auto);

        handler.Received.ShouldContain(e => e.Value == number1);
        handler.Received.ShouldContain(e => e.Value == number3);
        handler.Received.ShouldContain(e => e.Value == number3);
    }

    [Fact]
    public async Task ShouldPublishOnUnitOfWorkHookAfterCompleteWhenPublishModeIsOnUnitOfWorkComplete()
    {
        // OnUnitOfWorkComplete → AfterComplete (publish to broker after tx commits)

        var handler = fixture.Service<TestEventHandler>();
        var uowManager = fixture.Service<IUnitOfWorkManager>();

        await using var uow = uowManager.Begin(cancellationToken: TestContext.Current.CancellationToken);

        await EventBus.PublishAsync(
            new TestEvent(Number),
            publishMode: DistributedEventPublishMode.OnUnitOfWorkComplete);

        handler.Received.ShouldNotContain(e => e.Value == Number);

        uow.AddHook(UnitOfWorkHookPoint.BeforeComplete, () =>
        {
            handler.Received.ShouldNotContain(e => e.Value == Number);
            return Task.CompletedTask;
        });

        uow.AddHook(UnitOfWorkHookPoint.AfterComplete, () =>
        {
            handler.Received.ShouldContain(e => e.Value == Number);
            return Task.CompletedTask;
        });

        await uow.CompleteAsync(CancellationToken.None);
    }

    [Fact]
    public async Task ShouldPublishOnUnitOfWorkHookBeforeCompleteWhenPublishModeIsOutbox()
    {
        // Outbox mode  → BeforeComplete (persist to store before tx commits)

        var handler = fixture.Service<TestEventHandler>();
        var uowManager = fixture.Service<IUnitOfWorkManager>();

        await using var uow = uowManager.Begin(cancellationToken: TestContext.Current.CancellationToken);

        await EventBus.PublishAsync(
            new TestEvent(Number),
            publishMode: DistributedEventPublishMode.Outbox);

        handler.Received.ShouldNotContain(e => e.Value == Number);

        uow.AddHook(UnitOfWorkHookPoint.BeforeComplete, () =>
        {
            handler.Received.ShouldContain(e => e.Value == Number);
            return Task.CompletedTask;
        });

        await uow.CompleteAsync(CancellationToken.None);
    }

    [Fact]
    public async Task ShouldPublishOnUnitOfWorkHookBeforeCompleteWhenPublishModeIsAutoAndOutboxEnabled()
    {
        // Outbox.UseFor matches all types and IsOutboxEnabled is true (see ConfigureAsync)
        var handler = fixture.Service<TestEventHandler>();
        var uowManager = fixture.Service<IUnitOfWorkManager>();

        await using var uow = uowManager.Begin(cancellationToken: TestContext.Current.CancellationToken);
        await EventBus.PublishAsync(new TestEvent(Number), publishMode: DistributedEventPublishMode.Auto);

        handler.Received.ShouldNotContain(e => e.Value == Number);

        uow.AddHook(UnitOfWorkHookPoint.BeforeComplete, () =>
        {
            // Outbox hooks BeforeComplete
            handler.Received.ShouldContain(e => e.Value == Number);
            return Task.CompletedTask;
        });

        await uow.CompleteAsync(CancellationToken.None);
    }

    [Fact]
    public async Task ShouldPublishOnUnitOfWorkHookAfterCompleteWhenPublishModeIsAutoAndOutboxDisabled()
    {
        var provider = await fixture.CreateServiceProviderAsync(async builder =>
        {
            await fixture.Configure(builder);
            builder.Services.Configure<DistributedEventBusOptions>(o =>
            {
                // DistributedEventBusBase.IsOutboxEnabled is false
                o.Outbox.UseFor = static _ => false;
            });
        });

        var eventBus = provider.GetRequiredService<IDistributedEventBus>();
        var handler = provider.GetRequiredService<TestEventHandler>();
        var uowManager = provider.GetRequiredService<IUnitOfWorkManager>();

        await using var uow = uowManager.Begin(cancellationToken: TestContext.Current.CancellationToken);
        await eventBus.PublishAsync(new TestEvent(Number), publishMode: DistributedEventPublishMode.Auto);

        handler.Received.ShouldNotContain(e => e.Value == Number);

        uow.AddHook(UnitOfWorkHookPoint.BeforeComplete, () =>
        {
            // Outbox hooks BeforeComplete
            handler.Received.ShouldNotContain(e => e.Value == Number);
            return Task.CompletedTask;
        });

        uow.AddHook(UnitOfWorkHookPoint.AfterComplete, () =>
        {
            // Outbox hooks BeforeComplete
            handler.Received.ShouldContain(e => e.Value == Number);
            return Task.CompletedTask;
        });

        await uow.CompleteAsync(CancellationToken.None);
    }

    [Fact]
    public async Task ShouldCaptureTenantIdFromContextAccessorOnPublish()
    {
        var tenantContextAccessor = fixture.Service<ITenantContextAccessor>();
        var tenantId = Guid.NewGuid();
        tenantContextAccessor.Set(new TenantContext(tenantId, "t-1", "T-1"));

        await EventBus.PublishAsync(new TestEvent(Number), publishMode: DistributedEventPublishMode.Immediate);

        ((DistributedEventBusImp) EventBus).LastEnvelopeTenantId.ShouldBe(tenantId);
    }

    [Fact]
    public async Task ShouldCaptureNullTenantIdWhenNoTenantContext()
    {
        var tenantContextAccessor = fixture.Service<ITenantContextAccessor>();
        tenantContextAccessor.Set(current: null);

        await EventBus.PublishAsync(new TestEvent(Number), publishMode: DistributedEventPublishMode.Immediate);

        ((DistributedEventBusImp) EventBus).LastEnvelopeTenantId.ShouldBeNull();
    }
}

public class DistributedEventBusBaseFixture : IntegrationTest
{
    public Task Configure(IHostApplicationBuilder builder) => ConfigureAsync(builder);

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

        return Task.CompletedTask;
    }
}

[Dependency(AutoRegister = false)]
public class DistributedEventBusImp(
    ILogger<DistributedEventBusBase> logger,
    IOptions<DistributedEventBusOptions> options,
    DistributedEventHandlerManager eventHandlerManager,
    DistributedEventProcessor eventProcessor,
    IUnitOfWorkManager unitOfWorkManager,
    ITenantContextAccessor tenantContextAccessor,
    IInboxStore inboxStore,
    IOutboxStore outboxStore)
    : DistributedEventBusBase(logger, options, eventHandlerManager, eventProcessor, unitOfWorkManager,
        tenantContextAccessor, inboxStore, outboxStore)
{
    protected FrozenDictionary<Type, ImmutableArray<IDistributedEventHandlerAdapter>> Handlers { get; set; } = null!;

    public Guid? LastEnvelopeTenantId { get; private set; }

    protected override async Task PublishToOutboxAsync(EventEnvelope envelope)
    {
        foreach (var handler in Handlers[envelope.PayloadType])
        {
            await handler.HandleAsync(envelope.Payload, new EventContext());
        }
    }

    protected override async Task PublishToMessageBrokerAsync(EventEnvelope envelope)
    {
        LastEnvelopeTenantId = envelope.TenantId;

        foreach (var handler in Handlers[envelope.PayloadType])
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
file class InMemoryOutboxStore : IOutboxStore
{
}

[Dependency(AutoRegister = false)]
file class InMemoryInboxStore : IInboxStore
{
}

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