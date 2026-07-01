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
    protected static string PublisherChannelName { get; } = "event-bus.publisher";
    protected RabbitMqClientFactory ClientFactory { get; } = clientFactory;
    protected RabbitMqEventBusOptions RabbitMqOptions { get; } = rabbitMqOptions.Value;

    protected virtual async ValueTask<RabbitMqClient> GetClientAsync()
    {
        return await ClientFactory.GetAsync(RabbitMqOptions.ConnectionName);
    }

    protected override async Task PublishToMessageBrokerAsync<T>(EventEnvelope<T> envelope)
    {
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
        using var lease = await client.RentChannelAsync(PublisherChannelName);

        await lease.Channel.BasicPublishAsync(
            RabbitMqOptions.ExchangeName,
            GetEventTopic<T>(),
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
                RabbitMqOptions.ExchangeName,
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

            foreach (var topic in eventQueue.Events.Select(x => x.Value.Event.Topic))
            {
                await lease.Channel.QueueBindAsync(
                    queueName,
                    RabbitMqOptions.ExchangeName,
                    topic);
            }

            var consumer = new AsyncEventingBasicConsumer(lease.Channel);

            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                var routingKey = eventArgs.RoutingKey;
                var properties = eventArgs.BasicProperties;
                var body = eventArgs.Body.ToArray();
                var eventType = properties.Type ?? throw new InvalidOperationException("Event type is null");

                if (!eventQueue.Events.TryGetValue(eventType, out var eventItem))
                {
                    //Exception
                    return;
                }

                var payload = JsonSerializer.Deserialize(body, eventItem.Event.Type)!;

                foreach (var handler in eventItem.Handlers)
                {
                    await handler.HandleAsync(payload);
                }

                var eventConsumer = ((AsyncEventingBasicConsumer) sender);// = consumer
                await eventConsumer.Channel.BasicAckAsync(eventArgs.DeliveryTag, false);
            };

            await lease.Channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer);
        }
    }
}