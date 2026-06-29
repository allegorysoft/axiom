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
    RabbitMqClientFactory clientFactory,
    IOptions<RabbitMqEventBusOptions> rabbitMqOptions,
    IUnitOfWorkManager unitOfWorkManager,
    DistributedEventHandlerFactory handlerFactory,
    IOptions<DistributedEventBusOptions> options,
    IInboxStore inboxStore,
    IOutboxStore outboxStore)
    : DistributedEventBusBase(unitOfWorkManager, handlerFactory, options, inboxStore, outboxStore)
{
    protected RabbitMqClientFactory ClientFactory { get; } = clientFactory;
    protected RabbitMqEventBusOptions RabbitMqOptions { get; } = rabbitMqOptions.Value;

    protected virtual async ValueTask<RabbitMqClient> GetClientAsync()
    {
        return await ClientFactory.GetAsync(RabbitMqOptions.ConnectionName);
    }

    protected override async Task PublishToMessageBrokerAsync<T>(T payload)
    {
        const string channelName = "event-bus.publisher";

        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload);

        var client = await GetClientAsync();
        using var lease = await client.RentChannelAsync(channelName);

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = Guid.NewGuid().ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            Headers = new Dictionary<string, object?>
            {
                ["trace-parent"] = Activity.Current?.Id,
            }
        };

        await lease.Channel.BasicPublishAsync(
            RabbitMqOptions.ExchangeName,
            EventNameAttribute.Get<T>(),
            false,
            properties,
            bytes);
    }

    public override async Task InitializeAsync()
    {
        // We should create uow, before handler invoke
        // We should create Activity, and use SetParent(traceparent) from coming event
        // Use "IntegrationEvent" suffix; `public record OrderCreatedIntegrationEvent(int OrderId);`

        var client = await GetClientAsync();
        using var lease = await client.RentChannelAsync("consumer");
        await lease.Channel.ExchangeDeclareAsync(RabbitMqOptions.ExchangeName, ExchangeType.Direct, true);

        // We should rent channel for each consumer
        var consumer = new AsyncEventingBasicConsumer(lease.Channel);
        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            var x = (AsyncEventingBasicConsumer) sender;
            var eventType = eventArgs.RoutingKey;
            var body = eventArgs.Body.ToArray();

            JsonSerializer.Deserialize(body, typeof(int));

            await x.Channel.BasicAckAsync(eventArgs.DeliveryTag, false);

            // We should spawn thread pool task for each receive,
            // Otherwise it uses ConsumerDispatchConcurrency value doesn't respect Qos.Prefetch value
            // await Task.Factory.StartNew(async () =>
            // {
            //     Console.WriteLine(eventType + ": processing");
            //     await Task.Delay(TimeSpan.FromSeconds(10));
            //     Console.WriteLine(eventType + ": processed");
            //     await channel.BasicAckAsync(eventArgs.DeliveryTag, false);
            // });
        };

        await lease.Channel.BasicConsumeAsync(
            queue: "queue-1",
            autoAck: false,
            consumer: consumer);
    }
}