using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Allegory.Axiom.EventBus.Distributed;
using Allegory.Axiom.EventBus.Distributed.Inbox;
using Allegory.Axiom.EventBus.Distributed.Outbox;
using Allegory.Axiom.RabbitMQ;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Allegory.Axiom.EventBus;

public class RabbitMqDistributedEventBus(
    ILogger<RabbitMqDistributedEventBus> logger,
    IOptions<DistributedEventBusOptions> options,
    DistributedEventHandlerManager eventHandlerManager,
    IUnitOfWorkManager unitOfWorkManager,
    IInboxStore inboxStore,
    IOutboxStore outboxStore,
    RabbitMqClientFactory clientFactory)
    : DistributedEventBusBase(logger, options, eventHandlerManager, unitOfWorkManager, inboxStore, outboxStore)
{
    protected static string PublisherChannelName { get; } = "event-bus.publisher";
    protected RabbitMqClientFactory ClientFactory { get; } = clientFactory;

    protected virtual async ValueTask<RabbitMqClient> GetClientAsync()
    {
        return await ClientFactory.GetAsync(Options.RabbitMq.ConnectionName);
    }

    protected override async Task PublishToMessageBrokerAsync<T>(EventEnvelope<T> envelope)
    {
        var descriptor = GetEventDescriptor<T>();
        var properties = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = envelope.Id.ToString(),
            Type = descriptor.Name,
        };

        if (!string.IsNullOrWhiteSpace(envelope.TraceParent))
        {
            properties.Headers = new Dictionary<string, object?>
            {
                ["traceparent"] = envelope.TraceParent,
            };
        }

        var client = await GetClientAsync();
        var bytes = JsonSerializer.SerializeToUtf8Bytes(envelope.Payload, descriptor.Type);
        using var lease = await client.RentChannelAsync(PublisherChannelName);

        await lease.Channel.BasicPublishAsync(
            Options.RabbitMq.ExchangeName!,
            descriptor.Topic,
            false,
            properties,
            bytes);
    }

    public override async Task InitializeAsync()
    {
        var client = await GetClientAsync();

        using (var lease = await client.RentChannelAsync(PublisherChannelName))
        {
            await lease.Channel.ExchangeDeclareAsync(
                Options.RabbitMq.ExchangeName
                ?? throw new InvalidOperationException("RabbitMQ exchange name cannot be null"),
                ExchangeType.Direct,
                durable: true,
                autoDelete: false);
        }

        foreach (var (queueName, eventQueue) in EventHandlerManager.Queues)
        {
            var lease = await client.RentChannelAsync(queueName);

            await lease.Channel.QueueDeclareAsync(
                queueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            foreach (var topic in eventQueue.Events.Select(x => x.Value.Descriptor.Topic))
            {
                await lease.Channel.QueueBindAsync(
                    queueName,
                    Options.RabbitMq.ExchangeName!,
                    topic);
            }

            var consumer = new AsyncEventingBasicConsumer(lease.Channel);

            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                var eventConsumer = (AsyncEventingBasicConsumer) sender;// = consumer
                var routingKey = eventArgs.RoutingKey;
                var properties = eventArgs.BasicProperties;
                var body = eventArgs.Body.ToArray();
                var eventType = properties.Type ?? throw new InvalidOperationException("Event type cannot be null");

                if (!eventQueue.Events.TryGetValue(eventType, out var eventEntry))
                {
                    await eventConsumer.Channel.BasicRejectAsync(eventArgs.DeliveryTag, false);
                    Logger.LogWarning(
                        "Rejected message from queue '{Queue}' because no event handler is registered for event type '{EventType}'. Routing key: '{RoutingKey}'. This usually indicates the queue is still bound to a routing key that is no longer configured",
                        queueName,
                        eventType,
                        routingKey);
                    return;
                }

                var payload = JsonSerializer.Deserialize(body, eventEntry.Descriptor.Type)!;

                foreach (var handler in eventEntry.Handlers)
                {
                    await handler.HandleAsync(payload);
                }

                await eventConsumer.Channel.BasicAckAsync(eventArgs.DeliveryTag, false);
            };

            await lease.Channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer);
        }
    }
}
