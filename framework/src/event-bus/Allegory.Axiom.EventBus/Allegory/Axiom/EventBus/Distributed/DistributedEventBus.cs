using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.EventBus.Distributed.Inbox;
using Allegory.Axiom.EventBus.Distributed.Outbox;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.EventBus.Distributed;

[Dependency(Strategy = RegistrationStrategy.TryAdd)]
public class DistributedEventBus(
    IOptions<DistributedEventBusOptions> options,
    DistributedEventHandlerManager eventHandlerManager,
    IUnitOfWorkManager unitOfWorkManager,
    IInboxStore inboxStore,
    IOutboxStore outboxStore)
    : DistributedEventBusBase(options, eventHandlerManager, unitOfWorkManager, inboxStore, outboxStore)
{
    protected FrozenDictionary<string, ImmutableArray<IEventHandler>> Handlers { get; set; } = null!;

    public override Task PublishAsync<T>(
        T payload,
        DistributedEventPublishMode publishMode = DistributedEventPublishMode.Auto)
    {
        return Handlers.ContainsKey(typeof(T).FullName!)
            ? base.PublishAsync(payload, publishMode)
            : Task.CompletedTask;
    }

    protected override Task PublishToMessageBrokerAsync<T>(EventEnvelope<T> envelope)
        => InvokeHandlersAsync<T>(envelope.Payload);

    protected override Task PublishToOutboxAsync<T>(EventEnvelope<T> envelope)
        => InvokeHandlersAsync<T>(envelope.Payload);

    protected virtual async Task InvokeHandlersAsync<T>(object payload)
    {
        foreach (var handler in Handlers[typeof(T).FullName!])
        {
            await handler.HandleAsync(payload);
        }
    }

    public override Task InitializeAsync()
    {
        var handlers = new Dictionary<string, ImmutableArray<IEventHandler>.Builder>();

        foreach (var queue in EventHandlerManager.Queues.Values)
        {
            foreach (var (key, eventHandlers) in queue.Handlers)
            {
                if (!handlers.TryGetValue(key, out var builder))
                {
                    builder = ImmutableArray.CreateBuilder<IEventHandler>();
                    handlers[key] = builder;
                }

                builder.AddRange(eventHandlers);
            }
        }

        Handlers = handlers.ToFrozenDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToImmutable());

        return Task.CompletedTask;
    }
}