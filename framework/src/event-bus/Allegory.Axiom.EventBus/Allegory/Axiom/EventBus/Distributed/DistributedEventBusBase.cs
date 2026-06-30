using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.EventBus.Distributed.Inbox;
using Allegory.Axiom.EventBus.Distributed.Outbox;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.EventBus.Distributed;

public abstract class DistributedEventBusBase : IDistributedEventBus, ISingletonService
{
    protected DistributedEventBusBase(
        IOptions<DistributedEventBusOptions> options,
        DistributedEventHandlerManager eventHandlerManager,
        IUnitOfWorkManager unitOfWorkManager,
        IInboxStore inboxStore,
        IOutboxStore outboxStore)
    {
        Options = options.Value;
        EventHandlerManager = eventHandlerManager;
        UnitOfWorkManager = unitOfWorkManager;
        OutboxStore = outboxStore;
        InboxStore = inboxStore;

        IsInboxEnabled = !(InboxStore is NullInboxStore || Options.Inbox.UseFor == null);
        IsOutboxEnabled = !(OutboxStore is NullOutboxStore || Options.Outbox.UseFor == null);
    }

    protected DistributedEventBusOptions Options { get; }
    protected DistributedEventHandlerManager EventHandlerManager { get; }
    protected IUnitOfWorkManager UnitOfWorkManager { get; }
    protected IOutboxStore OutboxStore { get; }
    protected IInboxStore InboxStore { get; }
    protected bool IsOutboxEnabled { get; }
    protected bool IsInboxEnabled { get; }

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

    // What if this package initialize after other package use "EventBus.Publish" ?
    public abstract Task InitializeAsync();
}