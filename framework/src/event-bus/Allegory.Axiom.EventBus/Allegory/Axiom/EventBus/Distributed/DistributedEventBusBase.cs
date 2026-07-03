using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.EventBus.Distributed.Inbox;
using Allegory.Axiom.EventBus.Distributed.Outbox;
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
        IInboxStore inboxStore,
        IOutboxStore outboxStore)
    {
        Logger = logger;
        Options = options.Value;
        EventHandlerManager = eventHandlerManager;
        EventProcessor = eventProcessor;
        UnitOfWorkManager = unitOfWorkManager;
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
        publishMode = GetPublishMode<T>(publishMode);
        var envelope = new EventEnvelope<T>
        {
            Id = Guid.NewGuid(),
            TraceParent = Activity.Current?.Id,
            Payload = payload,
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

    protected virtual DistributedEventPublishMode GetPublishMode<T>(DistributedEventPublishMode publishMode)
    {
        return publishMode switch
        {
            DistributedEventPublishMode.Auto => IsOutboxEnabled && Options.Outbox.UseFor!(typeof(T))
                ? DistributedEventPublishMode.Outbox
                : DistributedEventPublishMode.OnUnitOfWorkComplete,

            DistributedEventPublishMode.Outbox => IsOutboxEnabled
                ? DistributedEventPublishMode.Outbox
                : DistributedEventPublishMode.OnUnitOfWorkComplete,

            _ => publishMode
        };
    }

    protected virtual Task PublishToOutboxAsync<T>(EventEnvelope<T> envelope) where T : notnull
    {
        //Save to store

        return Task.CompletedTask;
    }

    protected abstract Task PublishToMessageBrokerAsync<T>(EventEnvelope<T> envelope) where T : notnull;

    protected virtual DistributedEventDescriptor GetEventDescriptor<T>()
    {
        // We can't use `Options.GetEvent<T>()` to retrieve the descriptor here,
        // because `T` may not have any registered handlers.
        // When publishing an event, having a registered handler is not required.

        return EventDescriptorCache.GetOrAdd(
            typeof(T),
            static (type, options) =>
            {
                var descriptor = options.Events.FirstOrDefault(f => f.Type == type)
                                 ?? new DistributedEventDescriptor
                                 {
                                     Name = typeof(T).FullName
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