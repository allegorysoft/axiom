using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.EventBus.Distributed.Inbox;
using Allegory.Axiom.EventBus.Distributed.Outbox;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
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
    IInboxStore inboxStore,
    IOutboxStore outboxStore,
    IServiceScopeFactory serviceScopeFactory)
    : DistributedEventBusBase(logger, options, eventHandlerManager, eventProcessor, unitOfWorkManager, inboxStore, outboxStore)
{
    protected IServiceScopeFactory ServiceScopeFactory { get; } = serviceScopeFactory;
    protected FrozenDictionary<Type, ImmutableArray<IDistributedEventHandlerAdapter>> Handlers { get; set; } = null!;

    public override async Task PublishAsync<T>(
        T payload,
        DistributedEventPublishMode publishMode = DistributedEventPublishMode.Auto)
    {
        if (!Handlers.ContainsKey(typeof(T)))
        {
            return;
        }

        publishMode = GetPublishMode<T>(publishMode);
        var envelope = new EventEnvelope<T>
        {
            Id = Guid.NewGuid(),
            Payload = payload,
        };

        switch (publishMode)
        {
            case DistributedEventPublishMode.Immediate:
                await PublishToMessageBrokerAsync(envelope);
                return;

            case DistributedEventPublishMode.OnUnitOfWorkComplete:
                UnitOfWorkManager.Current!.AddHook(
                    UnitOfWorkHookPoint.BeforeComplete,
                    () => PublishToMessageBrokerAsync(envelope));
                return;

            case DistributedEventPublishMode.Outbox:
            case DistributedEventPublishMode.Auto:
            default:
                throw new ArgumentOutOfRangeException(nameof(publishMode), publishMode, null);
        }
    }

    protected override DistributedEventPublishMode GetPublishMode<T>(DistributedEventPublishMode publishMode)
    {
        if (publishMode == DistributedEventPublishMode.Immediate || UnitOfWorkManager.Current == null)
        {
            return DistributedEventPublishMode.Immediate;
        }

        return DistributedEventPublishMode.OnUnitOfWorkComplete;
    }

    protected override Task PublishToOutboxAsync<T>(EventEnvelope<T> envelope)
    {
        throw new UnreachableException("Outbox publishing cannot be used with the in-process event bus.");
    }

    protected override async Task PublishToMessageBrokerAsync<T>(EventEnvelope<T> envelope)
    {
        using var scope = ServiceScopeFactory.CreateScope();

        var context = new EventContext
        {
            Id = envelope.Id,
            CancellationToken = CancellationToken.None,
            ServiceProvider = scope.ServiceProvider
        };

        foreach (var handler in Handlers[typeof(T)])
        {
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