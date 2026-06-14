using System;
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
        IUnitOfWorkManager unitOfWorkManager,
        DistributedEventHandlerFactory handlerFactory,
        IOptions<DistributedEventBusOptions> options,
        IInboxStore inboxStore,
        IOutboxStore outboxStore)
    {
        UnitOfWorkManager = unitOfWorkManager;
        HandlerFactory = handlerFactory;
        Options = options.Value;
        OutboxStore = outboxStore;
        InboxStore = inboxStore;

        IsInboxEnabled = !(InboxStore is NullInboxStore || Options.Inbox.UseFor == null);
        IsOutboxEnabled = !(OutboxStore is NullOutboxStore || Options.Outbox.UseFor == null);
    }

    protected IUnitOfWorkManager UnitOfWorkManager { get; }
    protected DistributedEventHandlerFactory HandlerFactory { get; }
    protected DistributedEventBusOptions Options { get; }
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

        switch (publishMode)
        {
            case DistributedEventPublishMode.Immediate:
                await PublishToMessageBrokerAsync(payload);
                return;

            case DistributedEventPublishMode.OnUnitOfWorkComplete:
                if (UnitOfWorkManager.Current is null)
                {
                    await PublishToMessageBrokerAsync(payload);

                }
                else
                {
                    UnitOfWorkManager.Current.AddHook(
                        UnitOfWorkHookPoint.AfterComplete,
                        () => PublishToMessageBrokerAsync(payload));
                }

                return;

            case DistributedEventPublishMode.Outbox:
                if (UnitOfWorkManager.Current is null)
                {
                    await PublishToOutboxAsync(payload);
                }
                else
                {
                    UnitOfWorkManager.Current.AddHook(
                        UnitOfWorkHookPoint.BeforeComplete,
                        () => PublishToOutboxAsync(payload));
                }

                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(publishMode), publishMode, null);
        }
    }

    protected virtual DistributedEventPublishMode GetPublishMode<T>(DistributedEventPublishMode publishMode)
    {
        return publishMode switch
        {
            DistributedEventPublishMode.Auto => IsOutboxEnabled && Options.Outbox.UseFor!.Invoke(typeof(T))
                ? DistributedEventPublishMode.Outbox
                : DistributedEventPublishMode.OnUnitOfWorkComplete,

            DistributedEventPublishMode.Outbox => IsOutboxEnabled
                ? DistributedEventPublishMode.Outbox
                : DistributedEventPublishMode.OnUnitOfWorkComplete,

            _ => publishMode
        };
    }

    protected virtual Task PublishToOutboxAsync<T>(T payload) where T : notnull
    {
        //Save to store

        return Task.CompletedTask;
    }

    protected abstract Task PublishToMessageBrokerAsync<T>(T payload) where T : notnull;

    public abstract Task InitializeAsync();
}