using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.EventBus.Distributed.Inbox;
using Allegory.Axiom.EventBus.Distributed.Outbox;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.EventBus.Distributed;

public abstract class DistributedEventBusBase : IDistributedEventBus, ISingletonService
{
    protected DistributedEventBusBase(
        ILogger<DistributedEventBusBase> logger,
        IOptions<DistributedEventBusOptions> options,
        DistributedEventHandlerManager eventHandlerManager,
        DistributedEventProcessor eventProcessor,
        IUnitOfWorkManager unitOfWorkManager,
        ITenantContextAccessor tenantContextAccessor,
        IInboxStore inboxStore,
        IOutboxStore outboxStore)
    {
        Logger = logger;
        Options = options.Value;
        EventHandlerManager = eventHandlerManager;
        EventProcessor = eventProcessor;
        UnitOfWorkManager = unitOfWorkManager;
        TenantContextAccessor = tenantContextAccessor;
        OutboxStore = outboxStore;
        InboxStore = inboxStore;

        IsInboxEnabled = !(InboxStore is NullInboxStore || Options.Inbox.UseFor == null);
        IsOutboxEnabled = !(OutboxStore is NullOutboxStore || Options.Outbox.UseFor == null);
    }

    protected ILogger<DistributedEventBusBase> Logger { get; }
    protected DistributedEventBusOptions Options { get; }
    protected DistributedEventHandlerManager EventHandlerManager { get; }
    protected DistributedEventProcessor EventProcessor { get; }
    protected IUnitOfWorkManager UnitOfWorkManager { get; }
    protected ITenantContextAccessor TenantContextAccessor { get; }
    protected IInboxStore InboxStore { get; }
    protected IOutboxStore OutboxStore { get; }
    protected bool IsInboxEnabled { get; }
    protected bool IsOutboxEnabled { get; }
    protected ConcurrentDictionary<Type, DistributedEventDescriptor> EventDescriptorCache { get; } = [];

    public virtual async Task PublishAsync<T>(
        T payload,
        DistributedEventPublishMode publishMode = DistributedEventPublishMode.Auto)
        where T : notnull
    {
        var payloadType = typeof(T) == typeof(object) ? payload.GetType() : typeof(T);
        publishMode = GetPublishMode(publishMode, payloadType);

        var envelope = new EventEnvelope
        {
            Id = Guid.NewGuid(),
            TraceParent = Activity.Current?.Id,
            TenantId = TenantContextAccessor.Current?.Id,
            Payload = payload,
            PayloadType = payloadType
        };

        switch (publishMode)
        {
            case DistributedEventPublishMode.Immediate:
                await PublishToMessageBrokerAsync(envelope);
                return;

            case DistributedEventPublishMode.OnUnitOfWorkComplete:
                if (UnitOfWorkManager.Current is null)
                {
                    await PublishToMessageBrokerAsync(envelope);
                }
                else
                {
                    UnitOfWorkManager.Current.AddHook(
                        UnitOfWorkHookPoint.AfterComplete,
                        () => PublishToMessageBrokerAsync(envelope));
                }

                return;

            case DistributedEventPublishMode.Outbox:
                if (UnitOfWorkManager.Current is null)
                {
                    await PublishToOutboxAsync(envelope);
                }
                else
                {
                    UnitOfWorkManager.Current.AddHook(
                        UnitOfWorkHookPoint.BeforeComplete,
                        () => PublishToOutboxAsync(envelope));
                }

                return;

            case DistributedEventPublishMode.Auto:
            default:
                throw new ArgumentOutOfRangeException(nameof(publishMode), publishMode, null);
        }
    }

    protected virtual DistributedEventPublishMode GetPublishMode(DistributedEventPublishMode publishMode, Type payloadType)
    {
        return publishMode switch
        {
            DistributedEventPublishMode.Auto => IsOutboxEnabled && Options.Outbox.UseFor!(payloadType)
                ? DistributedEventPublishMode.Outbox
                : DistributedEventPublishMode.OnUnitOfWorkComplete,

            DistributedEventPublishMode.Outbox => IsOutboxEnabled
                ? DistributedEventPublishMode.Outbox
                : DistributedEventPublishMode.OnUnitOfWorkComplete,

            _ => publishMode
        };
    }

    protected virtual Task PublishToOutboxAsync(EventEnvelope envelope)
    {
        //Save to store

        return Task.CompletedTask;
    }

    protected abstract Task PublishToMessageBrokerAsync(EventEnvelope envelope);

    protected virtual DistributedEventDescriptor GetEventDescriptor(Type type)
    {
        // We can't use `Options.GetEvent<T>()` to retrieve the descriptor here,
        // because `T` may not have any registered handlers.
        // When publishing an event, having a registered handler is not required.

        return EventDescriptorCache.GetOrAdd(
            type,
            static (type, options) =>
            {
                var descriptor = options.Events.FirstOrDefault(f => f.Type == type)
                                 ?? new DistributedEventDescriptor
                                 {
                                     Name = type.FullName
                                            ?? throw new InvalidOperationException("Event name cannot be null"),
                                     Topic = TopicNameAttribute.Get(type),
                                     Type = type,
                                     Handlers = ImmutableArray<Type>.Empty
                                 };

                return descriptor;
            }, Options);
    }

    public abstract Task InitializeAsync();

    // Check inbox is enabled and save to store
    // Use "IntegrationEvent" suffix; `OrderCreatedIntegrationEvent`
}