using System;
using System.Text.Json;
using System.Threading.Tasks;
using Allegory.Axiom.EventBus.Distributed;
using Allegory.Axiom.RabbitMQ;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;

namespace Allegory.Axiom.EventBus;

public class RabbitMqEventConsumer
{
    public RabbitMqEventConsumer(
        RabbitMqChannel channel,
        string queueName,
        EventQueue eventQueue,
        ILogger logger,
        DistributedEventProcessor processor)
    {
        QueueName = queueName;
        Channel = channel;
        EventQueue = eventQueue;
        Logger = logger;
        Processor = processor;

        Consumer = new AsyncEventingBasicConsumer(channel.Channel);
        Consumer.ReceivedAsync += OnReceivedAsync;
    }

    public AsyncEventingBasicConsumer Consumer { get; }
    protected string QueueName { get; }
    protected RabbitMqChannel Channel { get; }
    protected EventQueue EventQueue { get; }
    protected ILogger Logger { get; }
    protected DistributedEventProcessor Processor { get; }

    protected virtual async Task OnReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
    {
        var properties = eventArgs.BasicProperties;

        try 
        {
            eventArgs.CancellationToken.ThrowIfCancellationRequested();

            if (!Guid.TryParse(properties.MessageId, out var eventId))
            {
                throw new InvalidOperationException("Event id cannot be parsed");
            }

            var eventType = properties.Type ?? throw new InvalidOperationException("Event type cannot be null");

            string? traceparent = null;
            if (properties.Headers != null && properties.Headers.TryGetValue("traceparent", out var traceParentId))
            {
                traceparent = traceParentId!.ToString();
            }

            if (!EventQueue.Events.TryGetValue(eventType, out var eventEntry))
            {
                Logger.LogMissingHandler(QueueName, eventType, eventArgs.RoutingKey);
                await Consumer.Channel.BasicRejectAsync(eventArgs.DeliveryTag, false);
                return;
            }

            var payload = JsonSerializer.Deserialize(eventArgs.Body.Span, eventEntry.Descriptor.Type)!;

            await Processor.ProcessAsync(
                eventEntry,
                eventId,
                payload,
                traceparent: traceparent,
                cancellationToken: eventArgs.CancellationToken);

            await Consumer.Channel.BasicAckAsync(eventArgs.DeliveryTag, false);
        }
        catch (OperationCanceledException)
        {
            Logger.LogOperationCancelled(properties.MessageId);
            await Consumer.Channel.BasicRejectAsync(eventArgs.DeliveryTag, true);
        }
        catch (Exception e)
        {
            Logger.LogException(e);
            await Consumer.Channel.BasicRejectAsync(eventArgs.DeliveryTag, true);
        }
    }
}