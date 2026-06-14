using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Allegory.Axiom.EventBus.Distributed;
using Allegory.Axiom.RabbitMQ;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Allegory.Axiom.EventBus;

public class RabbitMqDistributedEventBus(
    RabbitMqClientFactory clientFactory,
    IOptions<RabbitMqEventBusOptions> rabbitMqOptions,
    IUnitOfWorkManager unitOfWorkManager,
    DistributedEventHandlerFactory handlerFactory,
    IOptions<DistributedEventBusOptions> options)
    : DistributedEventBusBase(unitOfWorkManager, handlerFactory, options)
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

        var client = await GetClientAsync();
        using var lease = await client.RentChannelAsync(channelName);

        var buffer = new ArrayBufferWriter<byte>(initialCapacity: 512);
        await using var writer = new Utf8JsonWriter(buffer);
        JsonSerializer.Serialize(writer, payload);

        var props = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = Guid.NewGuid().ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            Headers = new Dictionary<string, object?>
            {
                ["x-source"] = "order-service",
                ["x-version"] = "1.0",
                ["x-correlation-id"] = Guid.NewGuid().ToString(),
                //["x-retry-count"]  = 0
            }
        };

        await lease.Channel.BasicPublishAsync("axiom", "a", false, props, buffer.WrittenMemory);
    }

    public override async Task InitializeAsync()
    {
        // We should create uow, before handler invoke
        // We should create Activity, and use SetParent(traceparent) from coming event
        // Use "IntegrationEvent" suffix; `public record OrderCreatedIntegrationEvent(int OrderId);`

        var client = await GetClientAsync();
        using var publisherChannel = await client.RentChannelAsync("publisher");
    }
}