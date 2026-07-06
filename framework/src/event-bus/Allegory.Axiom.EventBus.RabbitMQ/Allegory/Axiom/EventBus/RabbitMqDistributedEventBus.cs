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
        await DefineExchangeAsync(connection);

        foreach (var (queueName, eventQueue) in EventHandlerManager.Queues)
        {
            var lease = await connection.RentChannelAsync(queueName);
            await DefineQueueAsync(lease.RabbitMqChannel, queueName, eventQueue);
            await DefineConsumerAsync(lease.RabbitMqChannel, queueName, eventQueue);
        }
    }

    protected virtual async Task DefineExchangeAsync(RabbitMqConnection connection)
    {
        using var lease = await connection.RentChannelAsync(PublisherChannelName);

        await lease.Channel.ExchangeDeclareAsync(
            Options.RabbitMq.ExchangeName ?? throw new InvalidOperationException("RabbitMQ exchange name cannot be null"),
            ExchangeType.Direct,
            durable: true,
            autoDelete: false);
    }

    protected virtual async Task DefineQueueAsync(
        RabbitMqChannel channel,
        string queueName,
        EventQueue eventQueue)
    {
        await channel.Channel.QueueDeclareAsync(
            queueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        foreach (var topic in eventQueue.Events.Select(x => x.Value.Descriptor.Topic))
        {
            await channel.Channel.QueueBindAsync(
                queueName,
                Options.RabbitMq.ExchangeName!,
                topic);
        }

        var queueOption = Options.RabbitMq.Queue.Get(queueName);
        await channel.Channel.BasicQosAsync(
            queueOption.PrefetchSize,
            queueOption.PrefetchCount,
            queueOption.Global);
    }

    protected virtual async Task DefineConsumerAsync(
        RabbitMqChannel channel,
        string queueName,
        EventQueue eventQueue)
    {
        var eventConsumer = new RabbitMqEventConsumer(channel, queueName, eventQueue, Logger, EventProcessor);

        await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: eventConsumer.Consumer);
    }
}