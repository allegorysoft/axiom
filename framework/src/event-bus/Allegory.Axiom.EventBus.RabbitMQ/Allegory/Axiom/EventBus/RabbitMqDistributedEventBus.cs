using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Allegory.Axiom.EventBus.Distributed;
using Allegory.Axiom.EventBus.Distributed.Inbox;
using Allegory.Axiom.EventBus.Distributed.Outbox;
using Allegory.Axiom.RabbitMQ;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Allegory.Axiom.EventBus;

public class RabbitMqDistributedEventBus(
    IOptions<DistributedEventBusOptions> options,
    DistributedEventHandlerManager eventHandlerManager,
    IUnitOfWorkManager unitOfWorkManager,
    IInboxStore inboxStore,
    IOutboxStore outboxStore,
    RabbitMqClientFactory clientFactory,
    IOptions<RabbitMqEventBusOptions> rabbitMqOptions)
    : DistributedEventBusBase(options, eventHandlerManager, unitOfWorkManager, inboxStore, outboxStore)
{
    protected RabbitMqClientFactory ClientFactory { get; } = clientFactory;
    protected RabbitMqEventBusOptions RabbitMqOptions { get; } = rabbitMqOptions.Value;

    protected virtual async ValueTask<RabbitMqClient> GetClientAsync()
    {
        return await ClientFactory.GetAsync(RabbitMqOptions.ConnectionName);
    }

    protected override async Task PublishToMessageBrokerAsync<T>(EventEnvelope<T> envelope)
    {
        const string channelName = "event-bus.publisher";

        var bytes = JsonSerializer.SerializeToUtf8Bytes(envelope.Payload);
        var properties = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = envelope.Id.ToString(),
            Type = typeof(T).FullName!,
            Headers = new Dictionary<string, object?>
            {
                ["traceparent"] = envelope.TraceParent,
            }
        };

        var client = await GetClientAsync();
        using var lease = await client.RentChannelAsync(channelName);

        await lease.Channel.BasicPublishAsync(
            RabbitMqOptions.ExchangeName,
            EventNameAttribute.Get<T>(),
            false,
            properties,
            bytes);
    }

    public override async Task InitializeAsync()
    {
        var client = await GetClientAsync();
        var lease = await client.RentChannelAsync("consumer");

        await lease.Channel.ExchangeDeclareAsync(
            RabbitMqOptions.ExchangeName,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        await lease.Channel.QueueDeclareAsync(
            "queue-1",
            durable: true,
            exclusive: false,
            autoDelete: false);

        await lease.Channel.QueueBindAsync(
            "queue-1",
            RabbitMqOptions.ExchangeName,
            "Allegory.Axiom.EventBus.OrderCreated");

        var consumer = new AsyncEventingBasicConsumer(lease.Channel);

        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            var routingKey = eventArgs.RoutingKey;
            var properties = eventArgs.BasicProperties;
            var body = eventArgs.Body.ToArray();

            // if (!Options.Types.TryGetValue(properties.Type!, out var type))
            // {
            //     return;
            // }
            //
            // var payload = JsonSerializer.Deserialize(body, type);

            //(AsyncEventingBasicConsumer) sender = consumer
            await ((AsyncEventingBasicConsumer) sender).Channel.BasicAckAsync(eventArgs.DeliveryTag, false);
        };

        await lease.Channel.BasicConsumeAsync(
            queue: "queue-1",
            autoAck: false,
            consumer: consumer);
    }
}