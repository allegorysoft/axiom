using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Allegory.Axiom.RabbitMQ;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Allegory.Axiom.EventBus;

public class RabbitMqDistributedEventBus(
    RabbitMqClientFactory clientFactory,
    IUnitOfWorkManager unitOfWorkManager,
    DistributedEventHandlerFactory handlerFactory,
    IOptions<RabbitMqDistributedEventBusOptions> options)
    : DistributedEventBusBase(unitOfWorkManager, handlerFactory)
{
    protected RabbitMqClientFactory ClientFactory { get; } = clientFactory;
    protected RabbitMqDistributedEventBusOptions Options { get; } = options.Value;

    protected virtual async ValueTask<RabbitMqClient> GetClientAsync()
    {
        return await ClientFactory.GetAsync(Options.ConnectionName);
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

    // We should create uow, before handler invoke
    // We should create Activity, and use SetParent(traceparent) from coming event
    // Use "IntegrationEvent" suffix; `public record OrderCreatedIntegrationEvent(int OrderId);`
    public override async Task InitializeAsync()
    {
        var client = await ClientFactory.GetAsync("event-bus");
        using var publisherChannel = await client.RentChannelAsync("publisher");
    }
}