using System;
using System.Text.Json;
using System.Threading.Tasks;
using Allegory.Axiom.EventBus.Distributed;
using Allegory.Axiom.RabbitMQ;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Allegory.Axiom.EventBus;

public class RabbitMqEventConsumer
{
    public RabbitMqEventConsumer(
        RabbitMqChannel rabbitMqChannel,
        string queueName,
        EventQueue eventQueue,
        ILogger<RabbitMqEventConsumer> logger,
        DistributedEventProcessor eventProcessor,
        IHostApplicationLifetime applicationLifetime)
    {
        RabbitMqChannel = rabbitMqChannel;
        QueueName = queueName;
        EventQueue = eventQueue;
        Logger = logger;
        EventProcessor = eventProcessor;

        Consumer = new AsyncEventingBasicConsumer(rabbitMqChannel.Channel);
        Consumer.ReceivedAsync += OnReceivedAsync;

        // TODO: Call BasicCancelAsync to notify the broker to stop delivering messages for this consumer tag.
        // Consider implementing this within a hosted service that invokes
        // RabbitMqConnectionFactory.ShutdownGracefulAsync during application shutdown,
        // ensuring consumers are cancelled cleanly before the connection is closed.
        applicationLifetime.ApplicationStopping.Register(() => Consumer.ReceivedAsync -= OnReceivedAsync);
    }

    public AsyncEventingBasicConsumer Consumer { get; }
    public RabbitMqChannel RabbitMqChannel { get; }
    public string QueueName { get; }
    public EventQueue EventQueue { get; }
    protected ILogger<RabbitMqEventConsumer> Logger { get; }
    protected DistributedEventProcessor EventProcessor { get; }

    protected virtual async Task OnReceivedAsync(object sender, BasicDeliverEventArgs args)
    {
        var properties = args.BasicProperties;

        try
        {
            if (!GetEventQueueEntry(properties, out var eventQueueEntry))
            {
                Logger.LogMissingHandler(QueueName, properties.Type!, args.RoutingKey);
                await Consumer.Channel.BasicRejectAsync(args.DeliveryTag, false);
                return;
            }

            var id = GetId(properties);
            var traceparent = TryGetTraceParent(properties);
            var payload = JsonSerializer.Deserialize(args.Body.Span, eventQueueEntry.Descriptor.Type)!;

            using var processCounter = await EventProcessor.ProcessAsync(
                eventQueueEntry,
                id,
                payload,
                traceparent: traceparent,
                cancellationToken: args.CancellationToken);

            await Consumer.Channel.BasicAckAsync(args.DeliveryTag, false);
        }
        catch (OperationCanceledException)
        {
            Logger.LogOperationCancelled(properties.MessageId);
            await Consumer.Channel.BasicRejectAsync(args.DeliveryTag, true);
        }
        catch (Exception e)
        {
            // Should handle DLX
            Logger.LogException(e);
            await Consumer.Channel.BasicRejectAsync(args.DeliveryTag, true);
        }
    }

    protected virtual bool GetEventQueueEntry(IReadOnlyBasicProperties properties, out EventQueueEntry eventEntry)
    {
        var eventType = properties.Type ?? throw new InvalidOperationException("Event type cannot be null");
        return EventQueue.Events.TryGetValue(eventType, out eventEntry);
    }

    protected virtual Guid GetId(IReadOnlyBasicProperties properties)
    {
        return !Guid.TryParse(properties.MessageId, out var id)
            ? throw new InvalidOperationException("Event id cannot be parsed")
            : id;
    }

    protected virtual string? TryGetTraceParent(IReadOnlyBasicProperties properties)
    {
        if (properties.Headers != null && properties.Headers.TryGetValue("traceparent", out var traceParentId))
        {
            return traceParentId?.ToString();
        }

        return null;
    }
}