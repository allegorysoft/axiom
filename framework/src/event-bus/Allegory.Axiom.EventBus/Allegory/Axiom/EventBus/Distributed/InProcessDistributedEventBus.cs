using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.EventBus.Distributed.Inbox;
using Allegory.Axiom.EventBus.Distributed.Outbox;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.EventBus.Distributed;

[Dependency(Strategy = RegistrationStrategy.TryAdd)]
public class InProcessDistributedEventBus(
    ILogger<InProcessDistributedEventBus> logger,
    IOptions<DistributedEventBusOptions> options,
    DistributedEventHandlerManager eventHandlerManager,
    DistributedEventProcessor eventProcessor,
    IUnitOfWorkManager unitOfWorkManager,
    ITenantContextAccessor tenantContextAccessor,
    IInboxStore inboxStore,
    IOutboxStore outboxStore)
    : DistributedEventBusBase(logger, options, eventHandlerManager, eventProcessor, unitOfWorkManager, tenantContextAccessor, inboxStore, outboxStore)
{
    protected FrozenDictionary<Type, ImmutableArray<IDistributedEventHandlerAdapter>> Handlers { get; set; } = null!;

    public override async Task PublishAsync<T>(
        T payload,
        DistributedEventPublishMode publishMode = DistributedEventPublishMode.Auto)
    {
        var payloadType = typeof(T) == typeof(object) ? payload.GetType() : typeof(T);

        if (!Handlers.ContainsKey(payloadType))
        {
            return;
        }

        publishMode = GetPublishMode(publishMode, payloadType);
        var envelope = new EventEnvelope
        {
            Id = Guid.NewGuid(),
            TenantId = TenantContextAccessor.Current?.Id,
            Payload = payload,
            PayloadType = payloadType,
        };

        switch (publishMode)
        {
            case DistributedEventPublishMode.Immediate:
                await PublishToMessageBrokerAsync(envelope);
                return;

            case DistributedEventPublishMode.OnUnitOfWorkComplete:
                UnitOfWorkManager.RequiredCurrent.AddHook(
                    UnitOfWorkHookPoint.BeforeComplete,
                    () => PublishToMessageBrokerAsync(envelope));
                return;

            case DistributedEventPublishMode.Outbox:
            case DistributedEventPublishMode.Auto:
            default:
                throw new ArgumentOutOfRangeException(nameof(publishMode), publishMode, null);
        }
    }

    protected override DistributedEventPublishMode GetPublishMode(DistributedEventPublishMode publishMode, Type payloadType)
    {
        if (publishMode == DistributedEventPublishMode.Immediate || UnitOfWorkManager.Current == null)
        {
            return DistributedEventPublishMode.Immediate;
        }

        return DistributedEventPublishMode.OnUnitOfWorkComplete;
    }

    protected override Task PublishToOutboxAsync(EventEnvelope envelope)
    {
        throw new UnreachableException("Outbox publishing cannot be used with the in-process event bus.");
    }

    protected override async Task PublishToMessageBrokerAsync(EventEnvelope envelope)
    {
        var context = new EventContext
        {
            Id = envelope.Id
        };

        foreach (var handler in Handlers[envelope.PayloadType])
        {
            // We should change tenant if envelope.TenantId != tenantAccessor.Current
            // OnUnitOfWorkCompleted triggers might change tenant when they publish
            // Same apply for LocalEventBus
            await handler.HandleAsync(envelope.Payload, context);
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