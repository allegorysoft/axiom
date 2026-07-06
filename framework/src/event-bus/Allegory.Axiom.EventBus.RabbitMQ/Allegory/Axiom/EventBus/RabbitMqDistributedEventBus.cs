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
    DistributedEventProcessor eventProcessor,
    IUnitOfWorkManager unitOfWorkManager,
    IInboxStore inboxStore,
    IOutboxStore outboxStore,
    RabbitMqConnectionFactory connectionFactory)
    : DistributedEventBusBase(logger, options, eventHandlerManager, eventProcessor, unitOfWorkManager, inboxStore, outboxStore)
{
    protected static string PublisherChannelName { get; } = "event-bus.publisher";
    protected RabbitMqConnectionFactory ConnectionFactory { get; } = connectionFactory;

    protected virtual async ValueTask<RabbitMqConnection> GetConnectionAsync()
    {
        return await ConnectionFactory.GetAsync(Options.RabbitMq.ConnectionName);
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

        var connection = await GetConnectionAsync();
        var bytes = JsonSerializer.SerializeToUtf8Bytes(envelope.Payload, descriptor.Type);
        using var lease = await connection.RentChannelAsync(PublisherChannelName);

        await lease.Channel.BasicPublishAsync(
            Options.RabbitMq.ExchangeName!,
            descriptor.Topic,
            false,
            properties,
            bytes);
    }

    public override async Task InitializeAsync()
    {
        var connection = await GetConnectionAsync();

        using (var lease = await connection.RentChannelAsync(PublisherChannelName))
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
            var lease = await connection.RentChannelAsync(queueName);
            var queueOption = Options.RabbitMq.Queue.Get(queueName);

            await lease.Channel.QueueDeclareAsync(
                queueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            await lease.Channel.BasicQosAsync(
                queueOption.PrefetchSize,
                queueOption.PrefetchCount,
                queueOption.Global);

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
                //Enqueue task to thread pool
                //Handle dispose during mid-flight event process
                //Handle try-catch (dead-letter-queue)

                var eventConsumer = (AsyncEventingBasicConsumer) sender;// = consumer
                var routingKey = eventArgs.RoutingKey;
                var properties = eventArgs.BasicProperties;

                Guid.TryParse(properties.MessageId, out var eventId);
                var eventType = properties.Type ?? throw new InvalidOperationException("Event type cannot be null");
                string? traceparent = null;
                if (properties.Headers != null && properties.Headers.TryGetValue("traceparent", out var traceParentId))
                {
                    traceparent = traceParentId!.ToString();
                }

                if (!eventQueue.Events.TryGetValue(eventType, out var eventEntry))
                {
                    await eventConsumer.Channel.BasicRejectAsync(eventArgs.DeliveryTag, false);
                    Logger.LogMissingHandler(queueName, eventType, eventArgs.RoutingKey);
                    return;
                }

                var payload = JsonSerializer.Deserialize(eventArgs.Body.Span, eventEntry.Descriptor.Type)!;

                await EventProcessor.ProcessAsync(
                    eventEntry,
                    eventId,
                    payload,
                    traceparent: traceparent,
                    cancellationToken: eventArgs.CancellationToken);

                await eventConsumer.Channel.BasicAckAsync(eventArgs.DeliveryTag, false);
            };

            await lease.Channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer);
        }
    }
}