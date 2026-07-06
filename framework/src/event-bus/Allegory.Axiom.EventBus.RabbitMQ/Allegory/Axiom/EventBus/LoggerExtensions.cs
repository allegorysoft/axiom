using Microsoft.Extensions.Logging;

namespace Allegory.Axiom.EventBus;

internal static partial class LoggerExtensions
{
    [LoggerMessage(
        Message = "Rejected message from queue '{Queue}' because no event handler is registered for event type '{EventType}'. Routing key: '{RoutingKey}'. This usually indicates the queue is still bound to a routing key that is no longer configured"
        , Level = LogLevel.Warning)]
    public static partial void LogMissingHandler(
        this ILogger logger,
        string queue,
        string eventType,
        string routingKey);
}